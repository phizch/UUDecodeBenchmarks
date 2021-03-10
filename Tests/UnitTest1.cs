using System;
using System.Diagnostics;
using System.Text;
using UUDecode;
using Xunit;

namespace Tests
{
	public class UnitTest1
	{
		//string Decoded = "CatAndDogPetCatAndDogPet";
		//byte[] Data = Encoding.ASCII.GetBytes( "0V%T06YD1&]G4&5T0V%T06YD1&]G4&5T" );

		string Decoded = "Cats and dogs are pets!!";
		byte[] Data = Encoding.ASCII.GetBytes( "0V%T<R!A;F0@9&]G<R!A<F4@<&5T<R$A" );

		// Cats and dogs are pets!!
		// 0V%T<R!A;F0@9&]G<R!A<F4@<&5T<R$A

		byte[] GetDest() => new byte[32];

		void AssertCorrect( byte[] dest, int encByteCount )
		{
			var len = encByteCount * 6 / 8;
			var str = Encoding.ASCII.GetString( dest, 0, len );
			Assert.Equal( Decoded[0..len], str );
		}

		public void DecodeTest( int count, Action<byte[],byte[]> func )
		{
			var dest = GetDest();
			func( Data, dest );
			AssertCorrect( dest, count );
		}

		//public unsafe void DecodeTest( int count, Action<byte*, byte*> func )
		//{
		//	var dest = GetDest();
		//	func( Data, dest );
		//	var str = Encoding.ASCII.GetString( dest, 0, count );
		//}



		[Fact]
		public void DecodeVersion1()
		{
			DecodeTest( 4, (s,d) => UUEncoding.Scalar.DecodeVersion1( s, d ) );
		}

		[Fact]
		public void DecodeVersion2()
		{
			DecodeTest( 4, ( s, d ) => UUEncoding.Scalar.DecodeVersion2( s, d ) );

		}



		[Fact]
		public void DecodeTriplet()
		{
			DecodeTest( 4, ( s, d ) => UUEncoding.Scalar.DecodeTriplet( s, d ) );
		}



		[Fact]
		public void DecodeRef()
		{
			DecodeTest( 4, ( s, d ) =>
			{
				ref byte data = ref s[0];
				ref byte dest = ref d[0];

				UUEncoding.Scalar.DecodeRef( ref data, ref dest );
				} );
		}



		[Fact]
		public void DecodeRef_ver2()
		{
			DecodeTest( 4, ( s, d ) =>
			{
				ref byte data = ref s[0];
				ref byte dest = ref d[0];

				UUEncoding.Scalar.DecodeRef_ver2( ref data, ref dest );
			} );
		}



		[Fact]
		public void DecodeRef_ver3()
		{
			DecodeTest( 4, ( s, d ) =>
			{
				ref byte data = ref s[0];
				ref byte dest = ref d[0];

				UUEncoding.Scalar.DecodeRef_ver3( ref data, ref dest );
			} );
		}



		[Fact]
		public void Decode16Bytes()
		{
			DecodeTest( 16, ( s, d ) => UUEncoding.Vec128.Decode16Bytes( s, d ) );
		}



		[Fact]
		public void Decode16Bytes_v2()
		{
			DecodeTest( 16, ( s, d ) => UUEncoding.Vec128.Decode16Bytes_v2( s, d ) );
		}



		[Fact]
		public void Decode16Bytes_v3()
		{
			DecodeTest( 16, ( s, d ) => UUEncoding.Vec128.Decode16Bytes_v3( s, d ) );
		}



		[Fact]
		public void Decode16Bytes_v4()
		{
			DecodeTest( 16, ( s, d ) => UUEncoding.Vec128.Decode16Bytes_v4( s, d ) );
		}



		[Fact]
		public void Decode16Bytes_v5()
		{
			DecodeTest( 16, ( s, d ) => UUEncoding.Vec128.Decode16Bytes_v5( s, d ) );
		}



		[Fact]
		public unsafe void Decode16Bytes_v6()
		{
			DecodeTest( 16, ( s, d ) => UUEncoding.Vec128.Decode16Bytes_v6( s, d ) );
		}

		[Fact]
		public unsafe void Decode16Bytes_v7()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode16Bytes_v7( pData, pDest );
				AssertCorrect( dest, 16 );
			}
		}

		[Fact]
		public unsafe void Decode16Bytes_v8()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode16Bytes_v8( pData, pDest );
				AssertCorrect( dest, 16 );
			}
		}

		[Fact]
		public unsafe void Decode16Bytes_v9()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode16Bytes_v9( pData, pDest );
				AssertCorrect( dest, 16 );
			}
		}


		[Fact]
		public unsafe void Decode16Bytes_v10()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode16Bytes_v10( pData, pDest );
				AssertCorrect( dest, 16 );
			}
		}


		[Fact]
		public unsafe void Decode16Bytes_v10_Inline()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode16Bytes_v10_Inline( pData, pDest );
				AssertCorrect( dest, 16 );
			}
		}

		[Fact]
		public unsafe void Decode16Bytes_v10_Inline_LessVars()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode16Bytes_v10_Inline_LessVars( pData, pDest );
				AssertCorrect( dest, 16 );
			}
		}

		[Fact]
		public unsafe void Decode16Bytes_v11()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode16Bytes_v11( pData, pDest );
				AssertCorrect( dest, 16 );
			}
		}


		[Fact]
		public unsafe void Decode32Bytes_2x128()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode32Bytes_2x128( pData, pDest );
				AssertCorrect( dest, 32 );
			}
		}


		[Fact]
		public unsafe void Decode32Bytes_2x128Unrolled()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode32Bytes_2x128Unrolled( pData, pDest );
				AssertCorrect( dest, 32 );
			}
		}


		[Fact]
		public unsafe void Decode32Bytes_2x128Unrolled_v2()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec128.Decode32Bytes_2x128Unrolled_v2( pData, pDest );
				AssertCorrect( dest, 32 );
			}
		}


		[Fact]
		public unsafe void Decode32Bytes()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec256.Decode32Bytes( pData, pDest );
				AssertCorrect( dest, 32 );
			}
		}



		[Fact]
		public unsafe void Decode32Bytes_v2()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec256.Decode32Bytes_v2( pData, pDest );
				AssertCorrect( dest, 32 );
			}
		}



		[Fact]
		public unsafe void Decode32Bytes_v2_Permute()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec256.Decode32Bytes_v2_Permute( pData, pDest );
				AssertCorrect( dest, 32 );
			}
		}



		[Fact]
		public unsafe void Decode32Bytes_v2_temp_upper()
		{
			var dest = GetDest();

			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &dest[0] )
			{
				UUEncoding.Vec256.Decode32Bytes_v2_temp_upper( pData, pDest );
				AssertCorrect( dest, 32 );
			}
		}
	}
}
