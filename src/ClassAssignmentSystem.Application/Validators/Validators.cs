using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features.Admin.Commands;
using FluentValidation;

namespace ClassAssignmentSystem.Application.Validators;

public class RegisterTeacherValidator : AbstractValidator<RegisterTeacherCommand>
{
    public RegisterTeacherValidator()
    {
        RuleFor(x => x.Dto.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Dto.Password).NotEmpty().MinimumLength(8);
    }
}

public class RegisterStudentValidator : AbstractValidator<RegisterStudentCommand>
{
    public RegisterStudentValidator()
    {
        RuleFor(x => x.Dto.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Dto.Password).NotEmpty().MinimumLength(8);
    }
}

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
