using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;



namespace UUDecode
{
	public partial class UUEncoding
	{
		public class Vec256
		{
			public static bool IsSupported => Avx2.IsSupported;



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode32Bytes( byte* source, byte* dest )
			{
				Vector256<byte> maskA       = Vector256.Create( (uint)0x0000_003f ).AsByte();
				Vector256<byte> maskB       = Vector256.Create( (uint)0x0000_3f00 ).AsByte();
				Vector256<byte> maskC       = Vector256.Create( (uint)0x003f_0000 ).AsByte();
				Vector256<byte> maskD       = Vector256.Create( (uint)0x3f00_0000 ).AsByte();
				Vector256<byte> offsets     = Vector256.Create( (sbyte)-32 ).AsByte();

				Vector256<byte> vecSource   = Unsafe.As<byte, Vector256<byte>>( ref source[0] );
				Vector256<byte> subtracted  = Avx2.Add( vecSource, offsets );

				Vector256<byte> a     = Avx2.And( subtracted, maskA );
				Vector256<byte> b     = Avx2.And( subtracted, maskB );
				Vector256<byte> c     = Avx2.And( subtracted, maskC );
				Vector256<byte> d     = Avx2.And( subtracted, maskD );


				a = Avx2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Avx2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Avx2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Avx2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 


				a = Avx2.Or( a, b );
				c = Avx2.Or( c, d );
				a = Avx2.Or( a, c );                    // AA BB CC 00   AA BB CC 00


				// a contains: [C,B,A,0, F,E,D,0, I,H,G,0, L,K,J,0]
				// Shuffle bytes so that it becomes: [A,B,C, D,E,F, G,H,I, J,K,L, 0,0,0,0]


				//2,   1,  0,   6,  5,  4,   10,  9,  8,  14, 13, 12,  // 3, 7, 11, 15
				//	18, 17, 16,  22, 21, 20,     // 19

				var vecShuffle = Vector256.Create(
					0x02, 0x01, 0x00,   0x06, 0x05, 0x04,   0x0a, 0x09, 0x08,   0x0e, 0x0d, 0x0c,
					0x80, 0x80, 0x80, 0x80,	// 0x03, 0x07, 0x0b, 0x0f
					0x12, 0x11, 0x10,   0x16, 0x15, 0x14,   0x1a, 0x19, 0x18,   0x1e, 0x1d, 0x1c,
					0x80, 0x80, 0x80, 0x80 );  // 0x13, 0x17, 0x1b, 0x1f

				var vecBytes2 = Avx2.Shuffle( a, vecShuffle );

				Sse2.Store( dest, vecBytes2.GetLower() );
				Sse2.Store( dest+12, vecBytes2.GetUpper() );
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode32Bytes_v2( byte* source, byte* dest )
			{
				//Vector256<byte> vecSource   = Unsafe.As<byte, Vector256<byte>>( ref source[0] );
				Vector256<byte> vecSource   = Unsafe.As<byte, Vector256<byte>>( ref *source );
				Vector256<byte> subtracted  = Avx2.Add( vecSource, Vector256.Create( (sbyte)-32 ).AsByte() );

				Vector256<byte> a     = Avx2.And( subtracted, Vector256.Create( (uint)0x0000_003f ).AsByte() );
				Vector256<byte> b     = Avx2.And( subtracted, Vector256.Create( (uint)0x0000_3f00 ).AsByte() );
				Vector256<byte> c     = Avx2.And( subtracted, Vector256.Create( (uint)0x003f_0000 ).AsByte() );
				Vector256<byte> d     = Avx2.And( subtracted, Vector256.Create( (uint)0x3f00_0000 ).AsByte() );

				a = Avx2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Avx2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Avx2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Avx2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 

				a = Avx2.Or( a, b );
				c = Avx2.Or( c, d );
				a = Avx2.Or( a, c );                    // AA BB CC 00   AA BB CC 00

				var vecBytes2 = Avx2.Shuffle( a,
										  Vector256.Create(
										  0x02, 0x01, 0x00,   0x06, 0x05, 0x04,   0x0a, 0x09, 0x08,   0x0e, 0x0d, 0x0c,
										  0x80, 0x80, 0x80, 0x80,	// 0x03, 0x07, 0x0b, 0x0f
										  0x12, 0x11, 0x10,   0x16, 0x15, 0x14,   0x1a, 0x19, 0x18,   0x1e, 0x1d, 0x1c,
										  0x80, 0x80, 0x80, 0x80 )	// 0x13, 0x17, 0x1b, 0x1f
				);

				Sse2.Store( dest, vecBytes2.GetLower() );
				Sse2.Store( dest+12, vecBytes2.GetUpper() );
			}






			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode32Bytes_v2_Permute( byte* source, byte* dest )
			{
				//Vector256<byte> vecSource   = Unsafe.As<byte, Vector256<byte>>( ref source[0] );
				Vector256<byte> vecSource   = Unsafe.As<byte, Vector256<byte>>( ref *source );
				Vector256<byte> subtracted  = Avx2.Add( vecSource, Vector256.Create( (sbyte)-32 ).AsByte() );

				Vector256<byte> a     = Avx2.And( subtracted, Vector256.Create( (uint)0x0000_003f ).AsByte() );
				Vector256<byte> b     = Avx2.And( subtracted, Vector256.Create( (uint)0x0000_3f00 ).AsByte() );
				Vector256<byte> c     = Avx2.And( subtracted, Vector256.Create( (uint)0x003f_0000 ).AsByte() );
				Vector256<byte> d     = Avx2.And( subtracted, Vector256.Create( (uint)0x3f00_0000 ).AsByte() );

				a = Avx2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Avx2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Avx2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Avx2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 

				a = Avx2.Or( a, b );
				c = Avx2.Or( c, d );
				a = Avx2.Or( a, c );                    // AA BB CC 00   AA BB CC 00

				var vecBytes2 = Avx2.Shuffle( a,
										  Vector256.Create(
										  0x02, 0x01, 0x00,   0x06, 0x05, 0x04,   0x0a, 0x09, 0x08,   0x0e, 0x0d, 0x0c,
										  0x80, 0x80, 0x80, 0x80,	// 0x03, 0x07, 0x0b, 0x0f
										  0x12, 0x11, 0x10,   0x16, 0x15, 0x14,   0x1a, 0x19, 0x18,   0x1e, 0x1d, 0x1c,
										  0x80, 0x80, 0x80, 0x80 )	// 0x13, 0x17, 0x1b, 0x1f
				);

				vecBytes2 = Avx2.PermuteVar8x32( vecBytes2.AsInt32(), Vector256.Create( 0, 1, 2, 4,5,6, 3,7 ) ).AsByte();
				Avx.Store( dest, vecBytes2 );
			}




			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static unsafe void Decode32Bytes_v2_temp_upper( byte* source, byte* dest )
			{
				//Vector256<byte> vecSource   = Unsafe.As<byte, Vector256<byte>>( ref source[0] );
				Vector256<byte> vecSource   = Unsafe.As<byte, Vector256<byte>>( ref *source );
				Vector256<byte> subtracted  = Avx2.Add( vecSource, Vector256.Create( (sbyte)-32 ).AsByte() );

				Vector256<byte> a     = Avx2.And( subtracted, Vector256.Create( (uint)0x0000_003f ).AsByte() );
				Vector256<byte> b     = Avx2.And( subtracted, Vector256.Create( (uint)0x0000_3f00 ).AsByte() );
				Vector256<byte> c     = Avx2.And( subtracted, Vector256.Create( (uint)0x003f_0000 ).AsByte() );
				Vector256<byte> d     = Avx2.And( subtracted, Vector256.Create( (uint)0x3f00_0000 ).AsByte() );

				a = Avx2.ShiftLeftLogical( a.AsUInt32(), 18 ).AsByte();     // 00000000 00000000 00000000 00aaaaaa -> 00000000 aaaaaa00 00000000 00000000
				b = Avx2.ShiftLeftLogical( b.AsUInt32(), 4 ).AsByte();      // 00000000 00000000 00bbbbbb 00000000 -> 00000000 000000bb bbbb0000 00000000
				c = Avx2.ShiftRightLogical( c.AsUInt32(), 10 ).AsByte();    // 00000000 00cccccc 00000000 00000000 -> 00000000 00000000 0000cccc cc000000
				d = Avx2.ShiftRightLogical( d.AsUInt32(), 24 ).AsByte();    // 00dddddd 00000000 00000000 00000000 -> 00000000 00000000 00000000 00dddddd
																			//	After Or:							  00000000 aaaaaabb bbbbcccc ccdddddd
																			//                                        byte 3   byte 1   byte 2   byte 0

				// a uint: 0x00000000_00000000__00000000_00111111
				// b uint: 0x00000000_00000000__00111111_00000000 
				// c uint: 0x00000000_00111111__00000000_00000000 
				// d uint: 0x00111111_00000000__00000000_00000000 

				a = Avx2.Or( a, b );
				c = Avx2.Or( c, d );
				a = Avx2.Or( a, c );                    // AA BB CC 00   AA BB CC 00

				var vecBytes2 = Avx2.Shuffle( a,
										  Vector256.Create(
										  0x02, 0x01, 0x00,   0x06, 0x05, 0x04,   0x0a, 0x09, 0x08,   0x0e, 0x0d, 0x0c,
										  0x80, 0x80, 0x80, 0x80,	// 0x03, 0x07, 0x0b, 0x0f
										  0x12, 0x11, 0x10,   0x16, 0x15, 0x14,   0x1a, 0x19, 0x18,   0x1e, 0x1d, 0x1c,
										  0x80, 0x80, 0x80, 0x80 )	// 0x13, 0x17, 0x1b, 0x1f
				);

				var upper = vecBytes2.GetUpper();
				Sse2.Store( dest, vecBytes2.GetLower() );
				Sse2.Store( dest+12, upper );
			}
		}
	}
}
