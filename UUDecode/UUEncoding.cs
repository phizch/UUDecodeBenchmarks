using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;



namespace UUDecode
{
	public class UUEncoding
	{


		public class Scalar
		{

		}

		public class Vec128
		{
			public static bool IsSupported => Ssse3.IsSupported;

		}

		public class Vec256
		{
			public static bool IsSupported => Avx2.IsSupported;




		}



	}
}
