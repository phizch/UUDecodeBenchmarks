using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;



namespace UUDecode
{
	public partial class UUEncoding
	{
		public class Vec128
		{
			public static bool IsSupported => Ssse3.IsSupported;

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes( Span<byte> source, Span<byte> dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();

				Vector128<byte> toSubtract  = Vector128.Create( (byte)32 );

				Vector128<byte> vecSource   = Sse2.LoadVector128( (byte*)Unsafe.AsPointer( ref source[0] ) );

				Vector128<byte> subtracted  = Sse2.Subtract( vecSource, toSubtract );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//											00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                          byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				var tmp = stackalloc byte[16];
				Sse2.Store( tmp, vecBytes2 );
				var span = new Span<byte>( tmp, 12 );
				span.CopyTo( dest );

				// this is slow! It's better to write all 16 bytes!
				//var vecMoveMask = Vector128.Create( -1, -1, -1, 0 ).AsByte();
				//Sse2.MaskMove( vecBytes2, vecMoveMask, (byte*)Unsafe.AsPointer( ref dest[0] ) );
			}




			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v2( Span<byte> source, Span<byte> dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();

				Vector128<byte> toSubtract  = Vector128.Create( (byte)32 );

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>(ref source[0]);
				Vector128<byte> subtracted  = Sse2.Subtract( vecSource, toSubtract );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//											00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                          byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				var tmp = stackalloc byte[16];
				Sse2.Store( tmp, vecBytes2 );
				var span = new Span<byte>( tmp, 12 );
				span.CopyTo( dest );

				// this is slow! It's better to write all 16 bytes!
				//var vecMoveMask = Vector128.Create( -1, -1, -1, 0 ).AsByte();
				//Sse2.MaskMove( vecBytes2, vecMoveMask, (byte*)Unsafe.AsPointer( ref dest[0] ) );
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v3( Span<byte> source, Span<byte> dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//											00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                          byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				var tmp = stackalloc byte[16];
				Sse2.Store( tmp, vecBytes2 );
				var span = new Span<byte>( tmp, 12 );
				span.CopyTo( dest );

				// this is slow! It's better to write all 16 bytes!
				//var vecMoveMask = Vector128.Create( -1, -1, -1, 0 ).AsByte();
				//Sse2.MaskMove( vecBytes2, vecMoveMask, (byte*)Unsafe.AsPointer( ref dest[0] ) );
			}




			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v4( Span<byte> source, Span<byte> dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//											00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                          byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				var tmp = stackalloc byte[16];
				Sse2.Store( tmp, vecBytes2 );
				//var span = new Span<byte>( tmp, 12 );
				//span.CopyTo( dest );
				Unsafe.CopyBlock( ref dest[0], ref *tmp, 12 );


				// this is slow! It's better to write all 16 bytes!
				//var vecMoveMask = Vector128.Create( -1, -1, -1, 0 ).AsByte();
				//Sse2.MaskMove( vecBytes2, vecMoveMask, (byte*)Unsafe.AsPointer( ref dest[0] ) );
			}




			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v5( Span<byte> source, Span<byte> dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//											00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                          byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				//var tmp = stackalloc byte[16];
				//Sse2.Store( tmp, vecBytes2 );
				//Unsafe.CopyBlock( ref dest[0], ref *tmp, 12 );

				//You can do something like res.AsUInt64().ToScalar() to fetch the low 8 bytes,
				//then Sse2.ShiftRightLogical128BitLane(res, 8).AsUInt32().ToScalar() to get the other 4

				//	slow----	var first8 = vecBytes2.AsUInt64().ToScalar();
				//	slow----	var last4 = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();
				//	slow----	MemoryMarshal.Write( dest, ref first8 );
				//	slow----	MemoryMarshal.Write( dest[8..], ref last4 );

				ref byte outRef = ref dest[0];
				Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();


			}




			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v6( Span<byte> source, Span<byte> dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//											00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                          byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				//var tmp = stackalloc byte[16];
				//Sse2.Store( tmp, vecBytes2 );
				//Unsafe.CopyBlock( ref dest[0], ref *tmp, 12 );

				//You can do something like res.AsUInt64().ToScalar() to fetch the low 8 bytes,
				//then Sse2.ShiftRightLogical128BitLane(res, 8).AsUInt32().ToScalar() to get the other 4

				//	slow----	var first8 = vecBytes2.AsUInt64().ToScalar();
				//	slow----	var last4 = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();
				//	slow----	MemoryMarshal.Write( dest, ref first8 );
				//	slow----	MemoryMarshal.Write( dest[8..], ref last4 );


				//ref byte outRef = ref dest[0];
				//Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				//Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();

				//Debug.Assert( Sse41.IsSupported );
				//Debug.Assert( Sse42.IsSupported );


				ref byte outRef = ref dest[0];
				Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = vecBytes2.AsUInt32().GetElement( 2 );
			}






			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v7( byte* source, byte* dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//											00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                          byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				//var tmp = stackalloc byte[16];
				//Sse2.Store( tmp, vecBytes2 );
				//Unsafe.CopyBlock( ref dest[0], ref *tmp, 12 );

				//You can do something like res.AsUInt64().ToScalar() to fetch the low 8 bytes,
				//then Sse2.ShiftRightLogical128BitLane(res, 8).AsUInt32().ToScalar() to get the other 4

				//	slow----	var first8 = vecBytes2.AsUInt64().ToScalar();
				//	slow----	var last4 = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();
				//	slow----	MemoryMarshal.Write( dest, ref first8 );
				//	slow----	MemoryMarshal.Write( dest[8..], ref last4 );


				//ref byte outRef = ref dest[0];
				//Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				//Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();

				//Debug.Assert( Sse41.IsSupported );
				//Debug.Assert( Sse42.IsSupported );


				ref byte outRef = ref dest[0];
				Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = vecBytes2.AsUInt32().GetElement( 2 );
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v8( byte* source, byte* dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//											00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                          byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				//var tmp = stackalloc byte[16];
				//Sse2.Store( tmp, vecBytes2 );
				//Unsafe.CopyBlock( ref dest[0], ref *tmp, 12 );

				//You can do something like res.AsUInt64().ToScalar() to fetch the low 8 bytes,
				//then Sse2.ShiftRightLogical128BitLane(res, 8).AsUInt32().ToScalar() to get the other 4

				//	slow----	var first8 = vecBytes2.AsUInt64().ToScalar();
				//	slow----	var last4 = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();
				//	slow----	MemoryMarshal.Write( dest, ref first8 );
				//	slow----	MemoryMarshal.Write( dest[8..], ref last4 );


				//ref byte outRef = ref dest[0];
				//Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				//Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();

				//Debug.Assert( Sse41.IsSupported );
				//Debug.Assert( Sse42.IsSupported );


				//ref byte outRef = ref dest[0];
				//Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				//Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = vecBytes2.AsUInt32().GetElement( 2 );

				*(ulong*)dest       = vecBytes2.AsUInt64().ToScalar();
				*(uint*)(dest + 8)  = vecBytes2.AsUInt32().GetElement( 2 );
			}


			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v9( byte* source, byte* dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//											00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                          byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				//var tmp = stackalloc byte[16];
				//Sse2.Store( tmp, vecBytes2 );
				//Unsafe.CopyBlock( ref dest[0], ref *tmp, 12 );

				//You can do something like res.AsUInt64().ToScalar() to fetch the low 8 bytes,
				//then Sse2.ShiftRightLogical128BitLane(res, 8).AsUInt32().ToScalar() to get the other 4

				//	slow----	var first8 = vecBytes2.AsUInt64().ToScalar();
				//	slow----	var last4 = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();
				//	slow----	MemoryMarshal.Write( dest, ref first8 );
				//	slow----	MemoryMarshal.Write( dest[8..], ref last4 );


				//ref byte outRef = ref dest[0];
				//Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				//Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();

				//Debug.Assert( Sse41.IsSupported );
				//Debug.Assert( Sse42.IsSupported );


				//ref byte outRef = ref dest[0];
				//Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				//Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = vecBytes2.AsUInt32().GetElement( 2 );

				//*(ulong*)dest       = vecBytes2.AsUInt64().ToScalar();
				//*(uint*)(dest + 8)  = vecBytes2.AsUInt32().GetElement( 2 );

				Sse2.StoreScalar( (ulong*)dest, vecBytes2.AsUInt64() );
				Sse2.StoreScalar( (uint*)(dest+8), Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32() );

			}





			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v10( byte* source, byte* dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );


				//var tmp = stackalloc byte[16];
				//Sse2.Store( tmp, vecBytes2 );
				//Unsafe.CopyBlock( ref dest[0], ref *tmp, 12 );

				//You can do something like res.AsUInt64().ToScalar() to fetch the low 8 bytes,
				//then Sse2.ShiftRightLogical128BitLane(res, 8).AsUInt32().ToScalar() to get the other 4

				//	slow----	var first8 = vecBytes2.AsUInt64().ToScalar();
				//	slow----	var last4 = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();
				//	slow----	MemoryMarshal.Write( dest, ref first8 );
				//	slow----	MemoryMarshal.Write( dest[8..], ref last4 );


				//ref byte outRef = ref dest[0];
				//Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				//Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt32().ToScalar();

				//Debug.Assert( Sse41.IsSupported );
				//Debug.Assert( Sse42.IsSupported );


				//ref byte outRef = ref dest[0];
				//Unsafe.As<byte, ulong>( ref outRef ) = vecBytes2.AsUInt64().ToScalar();
				//Unsafe.As<byte, uint>( ref Unsafe.Add( ref outRef, 8 ) ) = vecBytes2.AsUInt32().GetElement( 2 );

				//*(ulong*)dest       = vecBytes2.AsUInt64().ToScalar();
				//*(uint*)(dest + 8)  = vecBytes2.AsUInt32().GetElement( 2 );

				//Sse2.StoreScalar( (ulong*)dest, vecBytes2.AsUInt64() );
				//Sse2.StoreScalar( (ulong*)(dest+8), Sse2.ShiftRightLogical128BitLane( vecBytes2, 8 ).AsUInt64() );
				Sse2.Store( dest, vecBytes2 );
			}



			//-- Decode16Bytes_v10_Inline is the same as Decode16Bytes_v10.
			//-- Decode16Bytes_v10 didn't initially have MethodImplOptions.AggressiveInlining
			//-- It's cleaner in terms of comments, so I'm leaving it here.


			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v10_Inline( byte* source, byte* dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

				Vector128<byte> a     = Sse2.And( subtracted, maskA );
				Vector128<byte> b     = Sse2.And( subtracted, maskB );
				Vector128<byte> c     = Sse2.And( subtracted, maskC );
				Vector128<byte> d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );
				var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );
				Sse2.Store( dest, vecBytes2 );
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v10_Inline_LessVars( byte* source, byte* dest )
			{
				// subtract 32 from each byte

				Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				Vector128<byte> subtracted  = Sse2.Add( vecSource, Vector128.Create( (sbyte)-32 ).AsByte() );

				Vector128<byte> a     = Sse2.And( subtracted, Vector128.Create( (uint)0x0000_003f ).AsByte() );
				Vector128<byte> b     = Sse2.And( subtracted, Vector128.Create( (uint)0x0000_3f00 ).AsByte() );
				Vector128<byte> c     = Sse2.And( subtracted, Vector128.Create( (uint)0x003f_0000 ).AsByte() );
				Vector128<byte> d     = Sse2.And( subtracted, Vector128.Create( (uint)0x3f00_0000 ).AsByte() );

				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 

				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00

				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecBytes2 = Ssse3.Shuffle( a, Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 ) );
				Sse2.Store( dest, vecBytes2 );
			}



			//-- by saucecontrol

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode16Bytes_v11( byte* source, byte* dest )
			{
				Vector128<sbyte> offsets = Vector128.Create((sbyte)-32);
				Vector128<sbyte> subtracted = Sse2.Add(offsets, Sse2.LoadVector128((sbyte*)source));

				Vector128<uint> mask = Vector128.Create((uint)0x00fc_0000); // 0000 0000  1111 1100  0000 0000 0000 0000
				Vector128<uint> res = Sse2.And(Sse2.ShiftLeftLogical(subtracted.AsUInt32(), 18), mask);
				mask = Sse2.ShiftRightLogical( mask, 6 );
				res = Sse2.Or( res, Sse2.And( Sse2.ShiftLeftLogical( subtracted.AsUInt32(), 4 ), mask ) );
				mask = Sse2.ShiftRightLogical( mask, 6 );
				res = Sse2.Or( res, Sse2.And( Sse2.ShiftRightLogical( subtracted.AsUInt32(), 10 ), mask ) );
				mask = Sse2.ShiftRightLogical( mask, 6 );
				res = Sse2.Or( res, Sse2.And( Sse2.ShiftRightLogical( subtracted.AsUInt32(), 24 ), mask ) );

				var vecBytes2 = Ssse3.Shuffle(res.AsByte(), Vector128.Create(2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, 0x80, 0x80, 0x80, 0x80));
				Sse2.Store( dest, vecBytes2 );
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode32Bytes_2x128( byte* source, byte* dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();


				for ( int i = 0; i < 2; i++ )
				{
					Vector128<byte> vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
					Vector128<byte> subtracted  = Sse2.Add( vecSource, offsets );

					Vector128<byte> a     = Sse2.And( subtracted, maskA );
					Vector128<byte> b     = Sse2.And( subtracted, maskB );
					Vector128<byte> c     = Sse2.And( subtracted, maskC );
					Vector128<byte> d     = Sse2.And( subtracted, maskD );


					a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
					b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
					c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
					d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																				//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																				//                                        byte 3   byte 1   byte 2   byte 0

					// a uint: 0x00000000_00000000__00000000_00111111
					// b uint: 0x00000000_00000000__00111111_00000000 
					// c uint: 0x00000000_00111111__00000000_00000000 
					// d uint: 0x00111111_00000000__00000000_00000000 


					a = Sse2.Or( a, b );
					c = Sse2.Or( c, d );
					a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


					// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
					// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

					var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

					var vecBytes2 = Ssse3.Shuffle( a, vecShuffle );
					Sse2.Store( dest, vecBytes2 );
					source += 16;
					dest += 12;
				}
			}


			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode32Bytes_2x128Unrolled( byte* source, byte* dest )
			{
				// subtract 32 from each byte

				Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource, subtracted, a, b, c, d, vecDone;

				vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				subtracted  = Sse2.Add( vecSource, offsets );

				a     = Sse2.And( subtracted, maskA );
				b     = Sse2.And( subtracted, maskB );
				c     = Sse2.And( subtracted, maskC );
				d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				vecDone = Ssse3.Shuffle( a, vecShuffle );
				Sse2.Store( dest, vecDone );
				source += 16;
				dest += 12;


				vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				subtracted  = Sse2.Add( vecSource, offsets );

				a     = Sse2.And( subtracted, maskA );
				b     = Sse2.And( subtracted, maskB );
				c     = Sse2.And( subtracted, maskC );
				d     = Sse2.And( subtracted, maskD );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				//vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				vecDone = Ssse3.Shuffle( a, vecShuffle );
				Sse2.Store( dest, vecDone );
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode32Bytes_2x128Unrolled_v2( byte* source, byte* dest )
			{
				// subtract 32 from each byte

				//Vector128<byte> maskA       = Vector128.Create( (uint)0x0000_003f ).AsByte();
				//Vector128<byte> maskB       = Vector128.Create( (uint)0x0000_3f00 ).AsByte();
				//Vector128<byte> maskC       = Vector128.Create( (uint)0x003f_0000 ).AsByte();
				//Vector128<byte> maskD       = Vector128.Create( (uint)0x3f00_0000 ).AsByte();
				//Vector128<byte> offsets     = Vector128.Create( (sbyte)-32 ).AsByte();

				Vector128<byte> vecSource, subtracted, a, b, c, d, vecDone;

				vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				subtracted  = Sse2.Add( vecSource, Vector128.Create( (sbyte)-32 ).AsByte() );

				a     = Sse2.And( subtracted, Vector128.Create( (uint)0x0000_003f ).AsByte() );
				b     = Sse2.And( subtracted, Vector128.Create( (uint)0x0000_3f00 ).AsByte() );
				c     = Sse2.And( subtracted, Vector128.Create( (uint)0x003f_0000 ).AsByte() );
				d     = Sse2.And( subtracted, Vector128.Create( (uint)0x3f00_0000 ).AsByte() );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				var vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				vecDone = Ssse3.Shuffle( a, vecShuffle );
				Sse2.Store( dest, vecDone );


				source += 16;
				dest += 12;


				vecSource   = Unsafe.As<byte, Vector128<byte>>( ref source[0] );
				subtracted  = Sse2.Add( vecSource, Vector128.Create( (sbyte)-32 ).AsByte() );

				a     = Sse2.And( subtracted, Vector128.Create( (uint)0x0000_003f ).AsByte() );
				b     = Sse2.And( subtracted, Vector128.Create( (uint)0x0000_3f00 ).AsByte() );
				c     = Sse2.And( subtracted, Vector128.Create( (uint)0x003f_0000 ).AsByte() );
				d     = Sse2.And( subtracted, Vector128.Create( (uint)0x3f00_0000 ).AsByte() );


				a = Sse2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Sse2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Sse2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Sse2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Sse2.Or( a, b );
				c = Sse2.Or( c, d );
				a = Sse2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]

				//vecShuffle = Vector128.Create( 2, 1, 0,   6, 5, 4,   10, 9, 8,  14, 13, 12,  0x80,0x80,0x80,0x80 );

				vecDone = Ssse3.Shuffle( a, vecShuffle );
				Sse2.Store( dest, vecDone );
			}
		}



	}
}
