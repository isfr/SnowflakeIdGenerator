using BenchmarkDotNet.Attributes;
using SnowflakeIdGenerator.Core;
using SnowflakeIdGenerator.Core.Contracts;

namespace SnowflakeExample
{
    [IterationCount(100)]
    [MinColumn, MaxColumn, IterationsColumn]
    public class SnowflakeGeneratorBenchmark
    {
        private IGenerator _generator = new Generator(1);

        [Benchmark]
        public long GenerateNextId() => _generator.GenerateNextId();
    }
}
