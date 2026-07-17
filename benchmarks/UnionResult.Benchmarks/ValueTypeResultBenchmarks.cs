using BenchmarkDotNet.Attributes;
using UnionResult;

namespace UnionResult.Benchmarks;

/// <summary>
/// Compares the two packages when T is a value type (int). UnionResult.Result&lt;T&gt;
/// stores its payload as `object`, so every success value gets boxed; OperationResult.Result&lt;T&gt;
/// stores T directly in a typed field, avoiding the boxing allocation.
///
/// Create and Read are benchmarked separately: a naive "create + immediately read back"
/// benchmark lets the JIT prove the box never escapes and eliminate it entirely, which would
/// hide the very difference this benchmark exists to show. Returning the raw boxed
/// reference from Create (instead of unboxing it) and reading from an already-built,
/// pre-escaped instance in Read keeps each phase honest.
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
    public object UnionResultCreateSuccess() => Result<int>.Success(SuccessValue).Value!;

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
