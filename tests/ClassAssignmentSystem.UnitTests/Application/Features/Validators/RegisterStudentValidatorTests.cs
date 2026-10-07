using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features.Admin.Commands;
using ClassAssignmentSystem.Application.Validators;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentValidation.TestHelper;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Validators;

public class RegisterStudentValidatorTests
{
    private readonly RegisterStudentValidator _validator = new();

    [Fact]
    public void Validate_WithValidDto_HasNoErrors()
    {
        var command = new RegisterStudentCommand(TestDataBuilder.CreateRegisterStudentDto());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEmail_HasValidationError()
    {
        var command = new RegisterStudentCommand(
            new RegisterStudentDto("John Student", "", TestDataBuilder.DefaultPassword));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Email);
    }

    [Fact]
    public void Validate_WithShortPassword_HasValidationError()
    {
        var command = new RegisterStudentCommand(
            new RegisterStudentDto("John Student", "student@test.com", "abc"));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Password);
    }
}
