using System;
using System.Runtime.CompilerServices;



namespace UUDecode
{
	public partial class UUEncoding
	{
		public class Scalar
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static void DecodeVersion1( ReadOnlySpan<byte> source, Span<byte> dest )
			{
				int a = ((source[0] - 32) & 0x3f) << 18;
				int b = ((source[1] - 32) & 0x3f) << 12;
				int c = ((source[2] - 32) & 0x3f) << 6;
				int d = ((source[3] - 32) & 0x3f);

				a |= c;
				b |= d;

				a |= b;

				dest[0] = (byte)(a >> 16);
				dest[1] = (byte)(a >> 8);
				dest[2] = (byte)(a);
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static void DecodeVersion2( ReadOnlySpan<byte> source, Span<byte> dest )
			{
				dest[0] = (byte)((((source[0]-32) & 0x3f) << 2) | (((source[1]-32) & 0x3f) >> 4));      // 0000_0011
				dest[1] = (byte)((((source[1]-32) & 0x3f) << 4) | (((source[2]-32) & 0x3f) >> 2));      // 1111_2222
				dest[2] = (byte)((((source[2]-32) & 0x3f) << 6) |  ((source[3]-32) & 0x3f));            // 2233_3333
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static void DecodeTriplet( ReadOnlySpan<byte> source, Span<byte> dest )
			{
				byte a = source[0];
				byte b = source[1];
				byte c = source[2];
				byte d = source[3];
				a -= 32;
				b -= 32;
				c -= 32;
				d -= 32;
				a &= 0x3f;
				b &= 0x3f;
				c &= 0x3f;
				d &= 0x3f;

				dest[0] = (byte)((a << 2) | (b >> 4));      // 0000_0011
				dest[1] = (byte)((b << 4) | (c >> 2));      // 1111_2222
				dest[2] = (byte)((c << 6) |  d);            // 2233_3333
			}




			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static void DecodeRef( ref byte source, ref byte dest )
			{
				int a =                      ((source - 32) & 0x3f) << 18;
				int b = ((Unsafe.Add( ref source, 1 ) - 32) & 0x3f) << 12;
				int c = ((Unsafe.Add( ref source, 2 ) - 32) & 0x3f) << 6;
				int d = ((Unsafe.Add( ref source, 3 ) - 32) & 0x3f);

				a |= c;
				b |= d;

				a |= b;

				dest =                      (byte)(a >> 16);
				Unsafe.Add( ref dest, 1 ) = (byte)(a >> 8);
				Unsafe.Add( ref dest, 2 ) = (byte)(a);
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static void DecodeRef_ver2( ref byte source, ref byte dest )
			{
				int a =                      ((source - 32) & 0x3f) << 18;
				int b = ((Unsafe.Add( ref source, 1 ) - 32) & 0x3f) << 12;
				int c = ((Unsafe.Add( ref source, 2 ) - 32) & 0x3f) << 6;
				int d = ((Unsafe.Add( ref source, 3 ) - 32) & 0x3f);

				a |= b | c | d;

				dest =                      (byte)(a >> 16);
				Unsafe.Add( ref dest, 1 ) = (byte)(a >> 8);
				Unsafe.Add( ref dest, 2 ) = (byte)(a);
			}



			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public static void DecodeRef_ver3( ref byte source, ref byte dest )
			{
				int a =                      ((source - 32) & 0x3f) << 18
				  | ((Unsafe.Add( ref source, 1 ) - 32) & 0x3f) << 12
				  | ((Unsafe.Add( ref source, 2 ) - 32) & 0x3f) << 6
				  | ((Unsafe.Add( ref source, 3 ) - 32) & 0x3f);


				dest =                      (byte)(a >> 16);
				Unsafe.Add( ref dest, 1 ) = (byte)(a >> 8);
				Unsafe.Add( ref dest, 2 ) = (byte)(a);
			}
		}



	}
}
