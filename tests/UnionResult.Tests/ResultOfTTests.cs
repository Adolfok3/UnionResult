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
    public void Value_OnSuccess_ReturnsTheBoxedValue()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Value_OnFailure_ReturnsTheException()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var result = Result<int>.Failure(exception);

        // Assert
        result.Value.Should().BeSameAs(exception);
    }

    [Fact]
    public void Value_OnDefault_IsNull()
    {
        // Act
        var result = default(Result<int>);

        // Assert
        result.Value.Should().BeNull();
    }

    [Fact]
    public void HasValue_OnSuccess_IsTrue()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void HasValue_OnFailure_IsTrue()
    {
        // Act
        var result = Result<int>.Failure(new InvalidOperationException("boom"));

        // Assert
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void HasValue_OnDefault_IsFalse()
    {
        // Act
        var result = default(Result<int>);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Failure_WithNullException_ThrowsArgumentNullException()
    {
        // Act
        var act = () => Result<int>.Failure(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
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
    public void Success_WithNullReferenceValue_IsReadableAsValue()
    {
        // The tag-based discriminator (rather than a "Value is T value" pattern check)
        // means a success built from a null reference round-trips correctly.

        // Act
        var result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AsValue().Should().BeNull();
    }

    [Fact]
    public void Default_OfValueTypeResult_IsNeitherSuccessNorFailure()
    {
        // An uninitialized (default) Result<T> has tag 0, which the tag-based
        // discriminator reports as neither success nor failure.

        // Act
        var result = default(Result<int>);
        var asValue = result.AsValue;
        var asException = result.AsException;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeFalse();
        asValue.Should().Throw<InvalidOperationException>();
        asException.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Success_WhenTIsAnExceptionType_IsStillClassifiedAsSuccess()
    {
        // The tag-based discriminator (rather than "Value is Exception") means
        // Result<T> can safely wrap a T that is itself an Exception (or subtype).

        // Arrange
        var payload = new ArgumentException("payload, not a real failure");

        // Act
        var result = Result<ArgumentException>.Success(payload);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.AsValue().Should().BeSameAs(payload);

        var asException = result.AsException;
        asException.Should().Throw<InvalidOperationException>();
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
