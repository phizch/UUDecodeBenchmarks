using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;

namespace UUDecode
{
	class Program
	{

		public static Summary RunBenchmarks()
		{
			var summary = BenchmarkRunner.Run<Benchmarks>();
			return summary;
		}



		static void Main( string[] args )
		{
			RunBenchmarks();


			Console.WriteLine( "Done. Press enter to exit" );
			while ( Console.ReadKey().Key != ConsoleKey.Enter ) ;
		}
	}
}
