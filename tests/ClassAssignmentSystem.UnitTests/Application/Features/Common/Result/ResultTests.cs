using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.Configurations;
using FluentAssertions;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Common.Result;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessfulResultWithoutError()
    {
        var result = ClassAssignmentSystem.Application.Configurations.Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ReturnsFailedResultWithError()
    {
        var error = Error.Validation("Code", "Description");

        var result = ClassAssignmentSystem.Application.Configurations.Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericSuccess_ReturnsValue()
    {
        var result = Result<Guid>.Success(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void GenericFailure_AccessingValueThrows()
    {
        var result = Result<string>.Failure(
            Error.Validation("Code", "Description"));

        result.IsFailure.Should().BeTrue();
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed result*");
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailureResult()
    {
        var error = Error.NotFound("NotFound", "Item not found");
        Result<int> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Error_Factories_SetExpectedErrorTypes()
    {
        Error.Validation("v", "d").Type.Should().Be(ErrorType.Validation);
        Error.NotFound("n", "d").Type.Should().Be(ErrorType.NotFound);
        Error.Conflict("c", "d").Type.Should().Be(ErrorType.Conflict);
        Error.Forbidden("f", "d").Type.Should().Be(ErrorType.Forbidden);
        Error.Failure("x", "d").Type.Should().Be(ErrorType.Failure);
    }
}
