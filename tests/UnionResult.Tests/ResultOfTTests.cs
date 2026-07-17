using System.Diagnostics;

namespace UnionResult.Tests;

public class ResultOfTTests
{
    [Fact]
    public void Success_ReturnsResultThatIsSuccessAndCarriesTheValue()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.AsValue().Should().Be(42);
    }

    [Fact]
    public void Failure_ReturnsResultThatIsFailureAndCarriesTheException()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var result = Result<int>.Failure(exception);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.AsException().Should().BeSameAs(exception);
    }

    [Fact]
    public void AsValue_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Failure(new InvalidOperationException("boom"));

        // Act
        var act = result.AsValue;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Result does not contain a value.");
    }

    [Fact]
    public void AsException_OnSuccess_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var act = () => result.AsException();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Result does not contain an exception.");
    }

    [Fact]
    public void Success_WithReferenceTypeValue_RoundTripsTheSameInstance()
    {
        // Arrange
        var payload = new List<string> { "a", "b" };

        // Act
        var result = Result<List<string>>.Success(payload);

        // Assert
        result.AsValue().Should().BeSameAs(payload);
    }

    [Fact]
    public void Success_WithDefaultStructValue_IsReadableAsValue()
    {
        // Act
        var result = Result<int>.Success(0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AsValue().Should().Be(0);
    }

    [Fact]
    public void Success_WithNullReferenceValue_IsReportedAsSuccessButAsValueThrows()
    {
        // Documents a current limitation: a null-pattern check ("Value is T value")
        // never matches null, so a success created from a null reference cannot be
        // read back via AsValue(), even though IsSuccess is true.

        // Act
        var result = Result<string?>.Success(null);
        var act = () => result.AsValue();

        // Assert
        result.IsSuccess.Should().BeTrue();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Default_OfValueTypeResult_IsReportedAsSuccessButAsValueThrows()
    {
        // Documents a current limitation: an uninitialized (default) Result<T> has a
        // null backing Value, so it looks like success (no exception present) but
        // AsValue() cannot produce a T out of that null.

        // Act
        var result = default(Result<int>);
        var act = result.AsValue;

        // Assert
        result.IsSuccess.Should().BeTrue();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Success_WhenTIsAnExceptionType_IsMisclassifiedAsFailure()
    {
        // Documents a current limitation: the success/failure discriminator is based
        // solely on "Value is Exception", so Result<T> cannot safely wrap a T that is
        // itself an Exception (or subtype) — it always reads back as a failure.

        // Arrange
        var payload = new ArgumentException("payload, not a real failure");

        // Act
        var result = Result<ArgumentException>.Success(payload);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();

        // Both accessors still resolve to the same underlying instance.
        result.AsValue().Should().BeSameAs(payload);
        result.AsException().Should().BeSameAs(payload);
    }

    [Fact]
    public void Constructor_WithValue_IsEquivalentToSuccess()
    {
        // Act
        var result = new Result<int>(7);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AsValue().Should().Be(7);
    }

    [Fact]
    public void Constructor_WithException_IsEquivalentToFailure()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var result = new Result<int>(exception);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.AsException().Should().BeSameAs(exception);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Success_RoundTripsVariousIntValues(int value)
    {
        // Act
        var result = Result<int>.Success(value);

        // Assert
        result.AsValue().Should().Be(value);
    }

    [Fact]
    public void Success_WithComplexObjectGraph_PreservesReferenceEquality()
    {
        // Arrange
        var record = new Person("Ada", 36);

        // Act
        var result = Result<Person>.Success(record);

        // Assert
        result.AsValue().Should().Be(record);
        result.AsValue().Should().BeSameAs(record);
    }

    [Fact]
    public void Failure_PreservesTheConcreteExceptionType()
    {
        // Arrange
        var exception = new UnreachableException("Some error message");

        // Act
        var result = Result<int>.Failure(exception);

        // Assert
        result.AsException().Should().BeOfType<UnreachableException>();
    }

    private sealed record Person(string Name, int Age);
}
