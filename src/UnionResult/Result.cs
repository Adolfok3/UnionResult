using System.Runtime.CompilerServices;

namespace UnionResult;

/// <summary>
/// Manually implemented union (rather than the `union Result&lt;T&gt;(T, Exception)`
/// declaration sugar) so value-type T is stored in its own typed field instead of the
/// compiler's single boxing `object Value` field. Discrimination uses an explicit tag
/// instead of `Value is Exception`, and AsValue/AsException go through TryGetValue - the
/// C# union "non-boxing access pattern" - so success/failure checks and reads never box.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
[Union]
public readonly struct Result<T> : IUnion
{
    private readonly T _value;
    private readonly Exception? _exception;
    private readonly byte _tag;

    public Result(T value)
    {
        _value = value;
        _exception = null;
        _tag = 1;
    }

    public Result(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _value = default!;
        _exception = exception;
        _tag = 2;
    }

    public object? Value => _tag switch
    {
        1 => _value,
        2 => _exception,
        _ => null,
    };

    public bool HasValue => _tag != 0;

    public bool IsSuccess => _tag == 1;

    public bool IsFailure => _tag == 2;

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Exception exception) => new(exception);

    public bool TryGetValue(out T value)
    {
        value = _value!;
        return _tag == 1;
    }

    public bool TryGetValue(out Exception value)
    {
        value = _exception!;
        return _tag == 2;
    }

    public T AsValue() => TryGetValue(out T value)
            ? value
            : throw new InvalidOperationException(
                "Result does not contain a value.");

    public Exception AsException() => TryGetValue(out Exception value)
            ? value
            : throw new InvalidOperationException(
                "Result does not contain an exception.");
}

/// <summary>
/// Manually implemented union mirroring <see cref="Result{T}"/>'s tag-based approach.
/// There is no value-type case here, so boxing was never a concern for this type; the
/// rewrite keeps it consistent with <see cref="Result{T}"/> and guards against a null
/// exception being treated as success.
/// </summary>
[Union]
public readonly struct Result : IUnion
{
    private readonly Exception? _exception;
    private readonly byte _tag;

    public Result(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _exception = exception;
        _tag = 1;
    }

    public object? Value => _tag == 1 ? _exception : null;

    public bool HasValue => _tag != 0;

    public bool IsSuccess => _tag == 0;

    public bool IsFailure => _tag == 1;

    public static Result Success() => default;

    public static Result Failure(Exception exception) => new(exception);

    public bool TryGetValue(out Exception value)
    {
        value = _exception!;
        return _tag == 1;
    }

    public Exception AsException() => TryGetValue(out Exception value)
            ? value
            : throw new InvalidOperationException(
                "Result does not contain an exception.");
}
