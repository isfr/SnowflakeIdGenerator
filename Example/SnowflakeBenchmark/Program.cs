using BenchmarkDotNet.Running;

namespace SnowflakeExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SnowflakeGeneratorBenchmark>();
        }
    }
}
