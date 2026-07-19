using System.Runtime.CompilerServices;

namespace UnionResult;

/// <summary>
/// Represents the outcome of an operation that returns a value of type
/// <typeparamref name="T"/>: either a success carrying that value, or a failure
/// carrying the <see cref="Exception"/> that caused it. Use it in place of throwing for
/// failures that are an expected, ordinary outcome of the operation - the caller decides
/// how to react instead of having to catch an exception.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
/// <example>
/// <code>
/// Result&lt;int&gt; Divide(int a, int b) =>
///     b == 0
///         ? Result&lt;int&gt;.Failure(new DivideByZeroException())
///         : Result&lt;int&gt;.Success(a / b);
///
/// var result = Divide(10, 2);
/// var message = result.IsSuccess
///     ? $"Result: {result.AsValue()}"
///     : $"Failed: {result.AsException().Message}";
/// </code>
/// </example>
/// <remarks>
/// Because <see cref="Result{T}"/> is a C# union type, it also supports exhaustive
/// pattern matching: <c>result switch { int value => ..., Exception ex => ... }</c>.
/// </remarks>
[Union]
public readonly struct Result<T> : IUnion
{
    private readonly T _value;
    private readonly Exception? _exception;
    private readonly State _state;

    public Result(T value)
    {
        _value = value;
        _state = State.Success;
    }

    public Result(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _value = default!;
        _exception = exception;
        _state = State.Failure;
    }

    private enum State : byte
    {
        Empty,
        Success,
        Failure,
    }

    public object? Value => _state switch
    {
        State.Success => _value,
        State.Failure => _exception,
        _ => null,
    };

    public bool HasValue => _state != State.Empty;

    public bool IsSuccess => _state == State.Success;

    public bool IsFailure => _state == State.Failure;

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Exception exception) => new(exception);

    public bool TryGetValue(out T value)
    {
        value = _value!;
        return _state == State.Success;
    }

    public bool TryGetValue(out Exception value)
    {
        value = _exception!;
        return _state == State.Failure;
    }

    public T AsValue() => ResultAccessor.OrThrow(
        TryGetValue(out T value), value, "Result does not contain a value.");

    public Exception AsException() => ResultAccessor.OrThrow(
        TryGetValue(out Exception value), value, "Result does not contain an exception.");
}

/// <summary>
/// Represents the outcome of an operation that has no return value: either a plain
/// success, or a failure carrying the <see cref="Exception"/> that caused it. Use it in
/// place of throwing for failures that are an expected, ordinary outcome of the
/// operation - the caller decides how to react instead of having to catch an exception.
/// </summary>
/// <example>
/// <code>
/// Result Save(User user) =>
///     repository.TrySave(user, out var error)
///         ? Result.Success()
///         : Result.Failure(error);
///
/// var result = Save(user);
/// if (result.IsFailure)
/// {
///     logger.LogError(result.AsException(), "Failed to save user");
/// }
/// </code>
/// </example>
[Union]
public readonly struct Result : IUnion
{
    private readonly Exception? _exception;

    public Result(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _exception = exception;
    }

    public object? Value => _exception;

    public bool HasValue => _exception is not null;

    public bool IsSuccess => _exception is null;

    public bool IsFailure => _exception is not null;

    public static Result Success() => default;

    public static Result Failure(Exception exception) => new(exception);

    public bool TryGetValue(out Exception value)
    {
        value = _exception!;
        return _exception is not null;
    }

    public Exception AsException() => ResultAccessor.OrThrow(
        TryGetValue(out Exception value), value, "Result does not contain an exception.");
}

/// <summary>
/// Shared "throw if not found" helper for the TryGetValue-based accessors above, so
/// AsValue/AsException don't each repeat their own throw expression.
/// </summary>
file static class ResultAccessor
{
    public static TValue OrThrow<TValue>(bool found, TValue value, string message) =>
        found ? value : throw new InvalidOperationException(message);
}
