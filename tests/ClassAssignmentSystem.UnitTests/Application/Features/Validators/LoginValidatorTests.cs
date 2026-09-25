using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Validators;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentValidation.TestHelper;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Validators;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Validate_WithValidDto_HasNoErrors()
    {
        var dto = TestDataBuilder.CreateLoginDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEmail_HasValidationError()
    {
        var dto = new LoginDto("", TestDataBuilder.DefaultPassword);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyPassword_HasValidationError()
    {
        var dto = new LoginDto("student@test.com", "");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
