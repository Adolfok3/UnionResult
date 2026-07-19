<div align="center">
  <img src="https://raw.githubusercontent.com/Adolfok3/UnionResult/main/assets/icon.png" alt="UnionResult">
</div>

# UnionResult

[![NuGet](https://img.shields.io/nuget/v/UnionResult.svg)](https://www.nuget.org/packages/UnionResult/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![.NET 11](https://img.shields.io/badge/.NET-11.0--preview-512BD4)](https://dotnet.microsoft.com/)
[![C# union types](https://img.shields.io/badge/C%23-union%20types-informational)](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union)
[![main](https://github.com/Adolfok3/UnionResult/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/Adolfok3/UnionResult/actions/workflows/main.yml)
[![tests](https://img.shields.io/github/actions/workflow/status/Adolfok3/UnionResult/main.yml?branch=main&label=tests)](https://github.com/Adolfok3/UnionResult/actions/workflows/main.yml)
[![tests count](https://img.shields.io/badge/tests-34%20passing-brightgreen)](./tests/UnionResult.Tests)
[![codecov](https://codecov.io/gh/Adolfok3/UnionResult/branch/main/graph/badge.svg)](https://codecov.io/gh/Adolfok3/UnionResult)

A Result pattern implementation for .NET, built on top of C#'s new native `union` types.

## Why

`UnionResult` isn't about functional programming for its own sake - it exists to avoid unnecessary exception overhead in everyday application code. Throwing and catching exceptions is expensive and meant for truly exceptional situations, not for outcomes an operation can expect and a caller should routinely handle (validation failures, "not found", a downstream call that failed). `Result`/`Result<T>` let a method return both possible outcomes explicitly, so the caller decides how to react instead of wrapping every call in `try`/`catch`.

Several packages already implement this pattern for .NET. What sets `UnionResult` apart is the implementation: instead of a hand-rolled discriminated-union struct, it's built directly on top of C#'s new native `union` type (introduced as a preview language feature in .NET 11) - success/failure is a real, compiler-recognized union with exhaustive pattern-matching support, not a struct that merely looks like one.

## Requirements

Union types are a **preview C# language feature**. To use this package your project needs:

```xml
<TargetFramework>net11.0</TargetFramework>
<LangVersion>preview</LangVersion>
```

and the .NET 11 preview SDK.

## Installation

```bash
dotnet add package UnionResult
```

Or via the Package Manager Console:

```powershell
Install-Package UnionResult
```

## Usage

### `Result<T>` - an operation that returns a value

```csharp
using UnionResult;

Result<int> Divide(int a, int b) =>
    b == 0
        ? Result<int>.Failure(new DivideByZeroException())
        : Result<int>.Success(a / b);

var result = Divide(10, 2);

var message = result.IsSuccess
    ? $"Result: {result.AsValue()}"
    : $"Failed: {result.AsException().Message}";
```

### `Result` - an operation with no return value

```csharp
using UnionResult;

Result Save(User user) =>
    repository.TrySave(user, out var error)
        ? Result.Success()
        : Result.Failure(error);

var result = Save(user);

if (result.IsFailure)
{
    logger.LogError(result.AsException(), "Failed to save user");
}
```

### Implicit conversions

`Success`/`Failure` aren't the only way to build a result - since `Result<T>` and `Result` are real C# unions, a value of a case type converts implicitly, so you can just `return` it directly:

```csharp
using UnionResult;

Result<int> Divide(int a, int b)
{
    if (b == 0)
    {
        return new DivideByZeroException(); // implicitly converts to Result<int>
    }

    return a / b; // implicitly converts to Result<int>
}

Result<int> r = 42; // same conversion, assigned directly
```

The same applies to the failure case of the non-generic `Result` (there's no implicit conversion for "success with no value", so that side still calls `Result.Success()` explicitly):

```csharp
Result Save(User user)
{
    if (!repository.TrySave(user, out var error))
    {
        return error; // implicitly converts to Result
    }

    return Result.Success();
}
```

### A more realistic example: replacing exceptions at a service boundary

```csharp
public interface IUserRepository
{
    Result<User> GetById(int id);
}

public class UserService(IUserRepository repository)
{
    public string GetDisplayName(int id)
    {
        var result = repository.GetById(id);
        return result.IsSuccess ? result.AsValue().Name : "Unknown user";
    }
}
```

Here `GetById` communicates "this can fail" through its return type. The caller isn't forced to guess whether the repository throws, and no `try`/`catch` is needed just to keep the app running when a lookup fails.

### Pattern matching

Because `Result<T>` is a real C# union, it also supports exhaustive pattern matching:

```csharp
var message = result switch
{
    int value => $"Result: {value}",
    Exception ex => $"Failed: {ex.Message}",
};
```

### API reference

| Member                                                          | Description                                                                                        |
| --------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| `Result<T>.Success(T value)` / `Result.Success()`               | Creates a successful result.                                                                       |
| `Result<T>.Failure(Exception)` / `Result.Failure(Exception)`    | Creates a failed result. Throws `ArgumentNullException` if `exception` is `null`.                  |
| `IsSuccess` / `IsFailure`                                       | Whether the result is a success or a failure.                                                      |
| `AsValue()` _(`Result<T>` only)_                                | Returns the success value, or throws `InvalidOperationException` if the result is a failure.       |
| `AsException()`                                                 | Returns the failure's exception, or throws `InvalidOperationException` if the result is a success. |
| `TryGetValue(out T value)` / `TryGetValue(out Exception value)` | Non-throwing variants of `AsValue()`/`AsException()`.                                              |
| `HasValue`                                                      | Whether the result holds any case at all (`false` only for `default(Result<T>)`).                  |

## Benchmarks

`UnionResult` was benchmarked against [`Divino.OperationResult`](https://github.com/victorDivino/operationResult), a similar Result-pattern package, across three scenarios: a value-type payload (`int`), a reference-type payload (`Product`), and no payload at all. Each scenario measures creating a result (`Create`) and reading an already-built one (`Read`), for both the success and failure paths.

**Environment:** BenchmarkDotNet v0.15.8, Windows 11, AMD Ryzen 7 7800X3D 4.20GHz, .NET SDK 11.0.100-preview.5, in-process toolchain.

| Category                     | Operation      | UnionResult | OperationResult | Allocated  | Winner                  |
| ---------------------------- | -------------- | ----------: | --------------: | :--------: | ----------------------- |
| **Value type (int)**         | Create Success |   0.0044 ns |       0.0000 ns | 0 B (both) | tie (noise)             |
|                              | Create Failure |   0.2049 ns |       0.6265 ns | 0 B (both) | UnionResult (~3x)       |
|                              | Read Success   |   0.4157 ns |       0.0023 ns | 0 B (both) | OperationResult         |
|                              | Read Failure   |   0.6244 ns |       0.2201 ns | 0 B (both) | OperationResult (~3x)   |
| **Reference type (Product)** | Create Success |   0.1977 ns |       0.2250 ns | 0 B (both) | tie                     |
|                              | Create Failure |   0.2374 ns |       1.1618 ns | 0 B (both) | UnionResult (~5x)       |
|                              | Read Success   |   0.6298 ns |       0.2062 ns | 0 B (both) | OperationResult (~3x)   |
|                              | Read Failure   |   0.6286 ns |       0.2114 ns | 0 B (both) | OperationResult (~3x)   |
| **No payload**               | Create Success |   0.0001 ns |       0.0024 ns | 0 B (both) | tie (noise)             |
|                              | Create Failure |   0.2248 ns |       0.6071 ns | 0 B (both) | UnionResult (~2.7x)     |
|                              | Read Success   |   0.0327 ns |       0.0005 ns | 0 B (both) | tie (noise)             |
|                              | Read Failure   |   0.6227 ns |       0.2338 ns | 0 B (both) | OperationResult (~2.7x) |

**Takeaways:**

- Neither package allocates in any scenario measured.
- `UnionResult` matches or beats `OperationResult` on every `Create` scenario - notably ~3-5x faster creating a failure.
- `OperationResult` is consistently faster to _read_ an already-built result (~0.2-0.4 ns) - `AsValue()`/`AsException()` validate the result's state before returning, which is the cost of that safety check.
- Values under ~0.03 ns aren't meaningful differences - that's the measurement floor of the benchmark itself.

The benchmark source lives in [`benchmarks/UnionResult.Benchmarks`](./benchmarks/UnionResult.Benchmarks).

## License

MIT - see [LICENSE](./LICENSE).
