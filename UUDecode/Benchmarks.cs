using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UUDecode
{

	public class Benchmarks
	{


		[GlobalSetup]
		public void GlobalSetup()
		{
			//var expStr = "CatAndDogPetCatAndDogPet";
			Data = Encoding.ASCII.GetBytes( "0V%T06YD1&]G4&5T0V%T06YD1&]G4&5T" );
			Dest = new byte[32];
		}

		byte[] Data;
		byte[] Dest;


		public int Count { get; set; } = 10_000;


		#region Scalar


		[Benchmark]
		public int Decode1()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				var data = dataSpan;
				var dest = destSpan;
				UUEncoding.Scalar.DecodeVersion1( data, dest );
				data = data[4..];
				dest = dest[3..];
				UUEncoding.Scalar.DecodeVersion1( data, dest );
				data = data[4..];
				dest = dest[3..];
				UUEncoding.Scalar.DecodeVersion1( data, dest );
				data = data[4..];
				dest = dest[3..];
				UUEncoding.Scalar.DecodeVersion1( data, dest );
				data = data[4..];
				dest = dest[3..];
				res += destSpan[0];
			}
			return res;
		}



		[Benchmark]
		public int Decode2()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				var data = dataSpan;
				var dest = destSpan;
				UUEncoding.Scalar.DecodeVersion2( data, dest );
				data = data[4..];
				dest = dest[3..];
				UUEncoding.Scalar.DecodeVersion2( data, dest );
				data = data[4..];
				dest = dest[3..];
				UUEncoding.Scalar.DecodeVersion2( data, dest );
				data = data[4..];
				dest = dest[3..];
				UUEncoding.Scalar.DecodeVersion2( data, dest );
				data = data[4..];
				dest = dest[3..];
				res += destSpan[0];
			}
			return res;
		}



		[Benchmark]
		public int Decode3()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				var data = dataSpan;
				var dest = destSpan;

				UUEncoding.Scalar.DecodeTriplet( data, dest );
				data = data[4..];
				dest = dest[3..];
				
				UUEncoding.Scalar.DecodeTriplet( data, dest );
				data = data[4..];
				dest = dest[3..];
				
				UUEncoding.Scalar.DecodeTriplet( data, dest );
				data = data[4..];
				dest = dest[3..];
				
				UUEncoding.Scalar.DecodeTriplet( data, dest );
				data = data[4..];
				dest = dest[3..];
				
				res += destSpan[0];
			}
			return res;
		}



		[Benchmark]
		public int DecodeRef()
		{
			// Yikes this is FAST!
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();

			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				ref byte data = ref MemoryMarshal.GetReference( dataSpan );
				ref byte dest = ref MemoryMarshal.GetReference( destSpan );

				UUEncoding.Scalar.DecodeRef( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				UUEncoding.Scalar.DecodeRef( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				UUEncoding.Scalar.DecodeRef( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				UUEncoding.Scalar.DecodeRef( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				res += destSpan[0];
			}
			return res;
		}



		[Benchmark]
		public int DecodeRef_ver2()
		{
			// Yikes this is FAST!
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();

			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				ref byte data = ref MemoryMarshal.GetReference( dataSpan );
				ref byte dest = ref MemoryMarshal.GetReference( destSpan );

				UUEncoding.Scalar.DecodeRef_ver2( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				UUEncoding.Scalar.DecodeRef_ver2( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				UUEncoding.Scalar.DecodeRef_ver2( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				UUEncoding.Scalar.DecodeRef_ver2( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				res += destSpan[0];
			}
			return res;
		}



		[Benchmark]
		public int DecodeRef_ver3()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				ref byte data = ref MemoryMarshal.GetReference( dataSpan );
				ref byte dest = ref MemoryMarshal.GetReference( destSpan );

				UUEncoding.Scalar.DecodeRef_ver3( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				UUEncoding.Scalar.DecodeRef_ver3( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				UUEncoding.Scalar.DecodeRef_ver3( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				UUEncoding.Scalar.DecodeRef_ver3( ref data, ref dest );
				data = Unsafe.Add( ref data, 4 );
				dest = Unsafe.Add( ref dest, 3 );

				res += destSpan[0];
			}
			return res;
		}



		#endregion Scalar



		[Benchmark]
		public int Decode16Bytes()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				UUEncoding.Vec128.Decode16Bytes( dataSpan, destSpan );
				res += destSpan[0];
			}
			return res;
		}

		[Benchmark]
		public int Decode16Bytes_v2()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				UUEncoding.Vec128.Decode16Bytes_v2( dataSpan, destSpan );
				res += destSpan[0];
			}
			return res;
		}

		[Benchmark]
		public int Decode16Bytes_v3()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				UUEncoding.Vec128.Decode16Bytes_v3( dataSpan, destSpan );
				res += destSpan[0];
			}
			return res;
		}

		[Benchmark]
		public int Decode16Bytes_v4()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				UUEncoding.Vec128.Decode16Bytes_v4( dataSpan, destSpan );
				res += destSpan[0];
			}
			return res;
		}


		[Benchmark]
		public int Decode16Bytes_v5()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				UUEncoding.Vec128.Decode16Bytes_v5( dataSpan, destSpan );
				res += destSpan[0];
			}
			return res;
		}

		[Benchmark]
		public int Decode16Bytes_v6()
		{
			var dataSpan = Data.AsSpan();
			var destSpan = Dest.AsSpan();
			var res = 0;
			for ( int i = 0; i < Count; i++ )
			{
				UUEncoding.Vec128.Decode16Bytes_v6( dataSpan, destSpan );
				res += destSpan[0];
			}
			return res;
		}

		[Benchmark]
		public unsafe int Decode16Bytes_v7()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count; i++ )
				{
					UUEncoding.Vec128.Decode16Bytes_v7( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}


		[Benchmark]
		public unsafe int Decode16Bytes_v8()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count; i++ )
				{
					UUEncoding.Vec128.Decode16Bytes_v8( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}


		[Benchmark]
		public unsafe int Decode16Bytes_v9()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count; i++ )
				{
					UUEncoding.Vec128.Decode16Bytes_v9( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}


		[Benchmark]
		public unsafe int Decode16Bytes_v10()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count; i++ )
				{
					UUEncoding.Vec128.Decode16Bytes_v10( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}

		[Benchmark]
		public unsafe int Decode16Bytes_v10_Inline()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count; i++ )
				{
					UUEncoding.Vec128.Decode16Bytes_v10_Inline( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}


		[Benchmark]
		public unsafe int Decode16Bytes_v10_Inline_LessVars()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count; i++ )
				{
					UUEncoding.Vec128.Decode16Bytes_v10_Inline_LessVars( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}

		[Benchmark]
		public unsafe int Decode16Bytes_v11()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count; i++ )
				{
					UUEncoding.Vec128.Decode16Bytes_v11( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}



		[Benchmark]
		public unsafe int Decode32Bytes()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count/2; i++ )
				{
					UUEncoding.Vec256.Decode32Bytes( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}



		[Benchmark]
		public unsafe int Decode32Bytes_2x128()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count/2; i++ )
				{
					UUEncoding.Vec128.Decode32Bytes_2x128( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}



		[Benchmark]
		public unsafe int Decode32Bytes_2x128Unrolled()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count/2; i++ )
				{
					UUEncoding.Vec128.Decode32Bytes_2x128Unrolled( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}



		[Benchmark]
		public unsafe int Decode32Bytes_v2()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count/2; i++ )
				{
					UUEncoding.Vec256.Decode32Bytes_v2( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}


		[Benchmark]
		public unsafe int Decode32Bytes_2x128Unrolled_v2()
		{
			fixed ( byte* pData = &Data[0] )
			fixed ( byte* pDest = &Dest[0] )
			{
				var res = 0;
				for ( int i = 0; i < Count/2; i++ )
				{
					UUEncoding.Vec128.Decode32Bytes_2x128Unrolled_v2( pData, pDest );
					res += pDest[0];
				}
				return res;
			}
		}



	}
}
