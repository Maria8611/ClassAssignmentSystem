using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features.Admin.Commands;
using ClassAssignmentSystem.Application.Validators;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentValidation.TestHelper;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Validators;

public class RegisterTeacherValidatorTests
{
    private readonly RegisterTeacherValidator _validator = new();

    [Fact]
    public void Validate_WithValidDto_HasNoErrors()
    {
        var command = new RegisterTeacherCommand(TestDataBuilder.CreateRegisterTeacherDto());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyFullName_HasValidationError()
    {
        var command = new RegisterTeacherCommand(
            new RegisterTeacherDto("", "teacher@test.com", TestDataBuilder.DefaultPassword));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.FullName);
    }

    [Fact]
    public void Validate_WithInvalidEmail_HasValidationError()
    {
        var command = new RegisterTeacherCommand(
            new RegisterTeacherDto("Jane Teacher", "not-an-email", TestDataBuilder.DefaultPassword));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Email);
    }

    [Fact]
    public void Validate_WithShortPassword_HasValidationError()
    {
        var command = new RegisterTeacherCommand(
            new RegisterTeacherDto("Jane Teacher", "teacher@test.com", "short"));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Password);
    }
}
