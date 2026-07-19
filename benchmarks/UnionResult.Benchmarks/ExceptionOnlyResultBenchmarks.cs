using BenchmarkDotNet.Attributes;
using UnionResult;

namespace UnionResult.Benchmarks;

/// <summary>
/// Compares the non-generic Result from each package: no payload, just a
/// success/failure signal plus (on failure) the causing exception.
/// </summary>
[Config(typeof(InProcessConfig))]
public class ExceptionOnlyResultBenchmarks
{
    private static readonly InvalidOperationException FailureException = new("boom");

    private Result _unionSuccess;
    private Result _unionFailure;
    private OperationResult.Result _operationSuccess;
    private OperationResult.Result _operationFailure;

    [GlobalSetup]
    public void Setup()
    {
        _unionSuccess = Result.Success();
        _unionFailure = Result.Failure(FailureException);
        _operationSuccess = OperationResult.Result.Success();
        _operationFailure = OperationResult.Result.Error(FailureException);
    }

    [Benchmark]
    public bool UnionResultCreateSuccess() => Result.Success().IsSuccess;

    [Benchmark]
    public bool OperationResultCreateSuccess() => OperationResult.Result.Success().IsSuccess;

    [Benchmark]
    public Exception UnionResultCreateFailure() => Result.Failure(FailureException).AsException();

    [Benchmark]
    public Exception OperationResultCreateFailure() => OperationResult.Result.Error(FailureException).Exception!;

    [Benchmark]
    public bool UnionResultReadSuccess() => _unionSuccess.IsSuccess;

    [Benchmark]
    public bool OperationResultReadSuccess() => _operationSuccess.IsSuccess;

    [Benchmark]
    public Exception UnionResultReadFailure() => _unionFailure.AsException();

    [Benchmark]
    public Exception OperationResultReadFailure() => _operationFailure.Exception!;
}
