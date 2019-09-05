# Snowflake Ids Generator

.Net implementation of the [Twitter Id Generator](https://blog.twitter.com/engineering/en_us/a/2010/announcing-snowflake.html)

## Benchmark

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-8550U CPU 1.80GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100-preview8-013656
  [Host]     : .NET Core 2.2.6 (CoreCLR 4.6.27817.03, CoreFX 4.6.27818.02), 64bit RyuJIT
  Job-DEDOFX : .NET Core 2.2.6 (CoreCLR 4.6.27817.03, CoreFX 4.6.27818.02), 64bit RyuJIT

IterationCount=100  

```

|         Method |     Mean |     Error |    StdDev |      Min |      Max | Iterations |
|--------------- |---------:|----------:|----------:|---------:|---------:|-----------:|
| GenerateNextId | 241.4 ns | 0.0110 ns | 0.0324 ns | 241.3 ns | 241.5 ns |     100.00 |
