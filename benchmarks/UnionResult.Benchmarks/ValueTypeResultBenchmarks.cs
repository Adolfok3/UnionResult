using BenchmarkDotNet.Attributes;
using UnionResult;

namespace UnionResult.Benchmarks;

/// <summary>
/// Compares the two packages when T is a value type (int). Result&lt;T&gt; stores T and
/// Exception in their own typed fields behind a tag (the union "non-boxing access
/// pattern" via TryGetValue), so AsValue()/AsException() never box; OperationResult.Result&lt;T&gt;
/// likewise stores T directly in a typed field. Both packages still expose a boxing
/// `object? Value` property (required by IUnion for UnionResult; OperationResult has no
/// such requirement but its own `.Value` is already unboxed for value types), but neither
/// AsValue() nor a normal consumer needs to go through it.
///
/// Create and Read are benchmarked separately so the cost of producing a Result isn't
/// conflated with the cost of consuming one. Both go through the non-boxing accessor
/// (AsValue()/.Value) so the numbers reflect how the API is actually meant to be used.
/// </summary>
[Config(typeof(InProcessConfig))]
public class ValueTypeResultBenchmarks
{
    private static readonly int SuccessValue = 42;
    private static readonly InvalidOperationException FailureException = new("boom");

    private Result<int> _unionSuccess;
    private Result<int> _unionFailure;
    private OperationResult.Result<int> _operationSuccess;
    private OperationResult.Result<int> _operationFailure;

    [GlobalSetup]
    public void Setup()
    {
        _unionSuccess = Result<int>.Success(SuccessValue);
        _unionFailure = Result<int>.Failure(FailureException);
        _operationSuccess = OperationResult.Result.Success(SuccessValue);
        _operationFailure = OperationResult.Result.Error<int>(FailureException);
    }

    [Benchmark]
    public int UnionResultCreateSuccess() => Result<int>.Success(SuccessValue).AsValue();

    [Benchmark]
    public int OperationResultCreateSuccess() => OperationResult.Result.Success(SuccessValue).Value;

    [Benchmark]
    public Exception UnionResultCreateFailure() => Result<int>.Failure(FailureException).AsException();

    [Benchmark]
    public Exception OperationResultCreateFailure() => OperationResult.Result.Error<int>(FailureException).Exception!;

    [Benchmark]
    public int UnionResultReadSuccess() => _unionSuccess.AsValue();

    [Benchmark]
    public int OperationResultReadSuccess() => _operationSuccess.Value;

    [Benchmark]
    public Exception UnionResultReadFailure() => _unionFailure.AsException();

    [Benchmark]
    public Exception OperationResultReadFailure() => _operationFailure.Exception!;
}
