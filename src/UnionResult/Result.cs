namespace UnionResult;

public readonly union Result<T>(T, Exception)
{
    public bool IsSuccess => Value is not Exception;

    public bool IsFailure => Value is Exception;

    public T AsValue() => Value is T value
            ? value
            : throw new InvalidOperationException(
                "Result does not contain a value.");

    public Exception AsException() => Value is Exception ex
            ? ex
            : throw new InvalidOperationException(
                "Result does not contain an exception.");

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Exception exception) => new(exception);
}

public readonly union Result(Exception)
{
    public bool IsSuccess => Value is not Exception;

    public bool IsFailure => Value is Exception;

    public Exception AsException() => Value is Exception ex
            ? ex
            : throw new InvalidOperationException(
                "Result does not contain an exception.");

    public static Result Success() => default;

    public static Result Failure(Exception exception) => new(exception);
}