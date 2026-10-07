using ClassAssignmentSystem.Application.Common.Behaviors;
using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.Configurations;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features.Admin.Commands;
using ClassAssignmentSystem.Application.Validators;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using FluentValidation;
using MediatR;

using ResultType = ClassAssignmentSystem.Application.Configurations.Result;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Common.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNoValidatorsRegistered_CallsNext()
    {
        var behavior = new ValidationBehavior<DummyCommand, ResultType>(Array.Empty<IValidator<DummyCommand>>());
        var nextCalled = false;

        var result = await behavior.Handle(
            new DummyCommand(),
            (Result) =>
            {
                nextCalled = true;
                return Task.FromResult(ClassAssignmentSystem.Application.Configurations.Result.Success());
            },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsFailureWithoutCallingNext()
    {
        var validators = new IValidator<RegisterTeacherCommand>[] { new RegisterTeacherValidator() };
        var behavior = new ValidationBehavior<RegisterTeacherCommand, Result<UserDto>>(validators);
        var nextCalled = false;
        var invalidCommand = new RegisterTeacherCommand(
            new RegisterTeacherDto("", "bad-email", "short"));

        var result = await behavior.Handle(
            invalidCommand,
            (Result) =>
            {
                nextCalled = true;
                return Task.FromResult(Result<UserDto>.Success(new UserDto(
                    Guid.NewGuid(), "Name", "email@test.com", "Teacher", true, DateTime.UtcNow)));
            },
            CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_CallsNext()
    {
        var validators = new IValidator<RegisterTeacherCommand>[] { new RegisterTeacherValidator() };
        var behavior = new ValidationBehavior<RegisterTeacherCommand, Result<UserDto>>(validators);
        var nextCalled = false;
        var validCommand = new RegisterTeacherCommand(TestDataBuilder.CreateRegisterTeacherDto());
        var expected = Result<UserDto>.Success(new UserDto(
            Guid.NewGuid(), "Jane Teacher", "teacher@test.com", "Teacher", true, DateTime.UtcNow));

        var result = await behavior.Handle(
            validCommand,
            (Result) =>
            {
                nextCalled = true;
                return Task.FromResult(expected);
            },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be(expected);
    }

    private sealed record DummyCommand : IRequest<ResultType>;
}
