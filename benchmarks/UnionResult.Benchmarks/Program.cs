using BenchmarkDotNet.Running;
using UnionResult.Benchmarks;

BenchmarkRunner.Run(
[
    typeof(ValueTypeResultBenchmarks),
    typeof(ReferenceTypeResultBenchmarks),
    typeof(ExceptionOnlyResultBenchmarks),
]);
