using NSubstitute.ExceptionExtensions;

namespace UnionResult.Tests;

/// <summary>
/// Exercises Result/Result&lt;T&gt; the way a consuming application would: a
/// dependency mocked with NSubstitute returns a Result instead of throwing, and
/// business logic branches on IsSuccess/IsFailure.
/// </summary>
public class ResultUsageScenariosTests
{
    public interface IUserRepository
    {
        Result<User> GetById(int id);

        Result Save(User user);
    }

    [Fact]
    public void GetDisplayName_WhenRepositoryReturnsSuccess_ReturnsUserName()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();
        repository.GetById(1).Returns(Result<User>.Success(new User(1, "Ada Lovelace")));
        var service = new UserService(repository);

        // Act
        var name = service.GetDisplayName(1);

        // Assert
        name.Should().Be("Ada Lovelace");
        repository.Received(1).GetById(1);
    }

    [Fact]
    public void GetDisplayName_WhenRepositoryReturnsFailure_ReturnsFallback()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();
        repository.GetById(99).Returns(Result<User>.Failure(new KeyNotFoundException("user 99 not found")));
        var service = new UserService(repository);

        // Act
        var name = service.GetDisplayName(99);

        // Assert
        name.Should().Be("Unknown user");
    }

    [Fact]
    public void TrySave_WhenRepositorySucceeds_ReturnsTrueAndNoError()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();
        repository.Save(Arg.Any<User>()).Returns(Result.Success());
        var service = new UserService(repository);

        // Act
        var saved = service.TrySave(new User(2, "Grace Hopper"), out var error);

        // Assert
        saved.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void TrySave_WhenRepositoryFails_ReturnsFalseAndPropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();
        var dbException = new InvalidOperationException("constraint violation");
        repository.Save(Arg.Any<User>()).Returns(Result.Failure(dbException));
        var service = new UserService(repository);

        // Act
        var saved = service.TrySave(new User(3, "Grace Hopper"), out var error);

        // Assert
        saved.Should().BeFalse();
        error.Should().BeSameAs(dbException);
    }

    [Fact]
    public void Repository_ThrowingInsteadOfReturningFailure_IsNotSwallowedByTheService()
    {
        // Result formalizes *expected* failures; it must not mask a dependency that
        // throws instead of returning Result.Failure.

        // Arrange
        var repository = Substitute.For<IUserRepository>();
        repository.GetById(Arg.Any<int>()).Throws(new TimeoutException("db timeout"));
        var service = new UserService(repository);

        // Act
        var act = () => service.GetDisplayName(1);

        // Assert
        act.Should().Throw<TimeoutException>().WithMessage("db timeout");
    }

    [Fact]
    public void MultipleConfiguredCalls_EachArgumentYieldsItsOwnConfiguredResult()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();
        repository.GetById(1).Returns(Result<User>.Success(new User(1, "Ada Lovelace")));
        repository.GetById(2).Returns(Result<User>.Failure(new KeyNotFoundException()));
        var service = new UserService(repository);

        // Act
        var nameForFirstUser = service.GetDisplayName(1);
        var nameForSecondUser = service.GetDisplayName(2);

        // Assert
        nameForFirstUser.Should().Be("Ada Lovelace");
        nameForSecondUser.Should().Be("Unknown user");
    }

    public sealed record User(int Id, string Name);

    public sealed class UserService(IUserRepository repository)
    {
        public string GetDisplayName(int id)
        {
            var result = repository.GetById(id);
            return result.IsSuccess ? result.AsValue().Name : "Unknown user";
        }

        public bool TrySave(User user, out Exception? error)
        {
            var result = repository.Save(user);
            error = result.IsFailure ? result.AsException() : null;
            return result.IsSuccess;
        }
    }
}
