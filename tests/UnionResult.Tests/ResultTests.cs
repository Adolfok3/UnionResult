using System.Diagnostics;

namespace UnionResult.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsResultThatIsSuccess()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Failure_ReturnsResultThatIsFailure()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var result = Result.Failure(exception);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AsException_OnFailure_ReturnsTheOriginalExceptionInstance()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");
        var result = Result.Failure(exception);

        // Act
        var caught = result.AsException();

        // Assert
        caught.Should().BeSameAs(exception);
    }

    [Fact]
    public void AsException_OnSuccess_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var act = () => result.AsException();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Result does not contain an exception.");
    }

    [Fact]
    public void Default_BehavesAsSuccess()
    {
        // Act
        var result = default(Result);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Failure_WithNullException_ThrowsArgumentNullException()
    {
        // Act
        var act = () => Result.Failure(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(NotSupportedException))]
    public void Failure_PreservesTheConcreteExceptionType(Type exceptionType)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        // Act
        var result = Result.Failure(exception);

        // Assert
        result.AsException().Should().BeOfType(exceptionType);
    }

    [Fact]
    public void Failure_PreservesInnerExceptionAndMessage()
    {
        // Arrange
        var inner = new UnreachableException("inner failure");
        var exception = new InvalidOperationException("outer failure", inner);

        // Act
        var result = Result.Failure(exception);

        // Assert
        var caught = result.AsException();
        caught.Message.Should().Be("outer failure");
        caught.InnerException.Should().BeSameAs(inner);
    }
}
