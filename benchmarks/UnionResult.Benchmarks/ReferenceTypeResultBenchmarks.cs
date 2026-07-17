using BenchmarkDotNet.Attributes;
using UnionResult;

namespace UnionResult.Benchmarks;

/// <summary>
/// Compares the two packages when T is a reference type (Product). Neither package
/// needs to box a reference type, so this isolates the pure wrapping/reading overhead
/// of each Result shape from the boxing cost measured in <see cref="ValueTypeResultBenchmarks"/>.
/// Create and Read are benchmarked separately for the same reason as the value-type case:
/// it keeps the two costs (producing vs consuming a Result) from being conflated.
/// </summary>
[Config(typeof(InProcessConfig))]
public class ReferenceTypeResultBenchmarks
{
    private static readonly Product SuccessValue = new(1, "Keyboard", 199.90m);
    private static readonly InvalidOperationException FailureException = new("boom");

    private Result<Product> _unionSuccess;
    private Result<Product> _unionFailure;
    private OperationResult.Result<Product> _operationSuccess;
    private OperationResult.Result<Product> _operationFailure;

    [GlobalSetup]
    public void Setup()
    {
        _unionSuccess = Result<Product>.Success(SuccessValue);
        _unionFailure = Result<Product>.Failure(FailureException);
        _operationSuccess = OperationResult.Result.Success(SuccessValue);
        _operationFailure = OperationResult.Result.Error<Product>(FailureException);
    }

    [Benchmark]
    public object UnionResultCreateSuccess() => Result<Product>.Success(SuccessValue).Value!;

    [Benchmark]
    public Product OperationResultCreateSuccess() => OperationResult.Result.Success(SuccessValue).Value!;

    [Benchmark]
    public Exception UnionResultCreateFailure() => Result<Product>.Failure(FailureException).AsException();

    [Benchmark]
    public Exception OperationResultCreateFailure() => OperationResult.Result.Error<Product>(FailureException).Exception!;

    [Benchmark]
    public Product UnionResultReadSuccess() => _unionSuccess.AsValue();

    [Benchmark]
    public Product OperationResultReadSuccess() => _operationSuccess.Value!;

    [Benchmark]
    public Exception UnionResultReadFailure() => _unionFailure.AsException();

    [Benchmark]
    public Exception OperationResultReadFailure() => _operationFailure.Exception!;
}
