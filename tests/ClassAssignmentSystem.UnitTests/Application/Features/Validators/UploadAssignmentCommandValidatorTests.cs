using ClassAssignmentSystem.Application.Assignments.Commands.UploadAssignment;
using ClassAssignmentSystem.Application.Configurations;
using ClassAssignmentSystem.Application.Features.Assignments.Commands.UploadAssignment;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Validators;

public class UploadAssignmentCommandValidatorTests
{
    private readonly UploadAssignmentCommandValidator _validator;

    public UploadAssignmentCommandValidatorTests()
    {
        _validator = new UploadAssignmentCommandValidator(Options.Create(new BlobStorageOptions
        {
            MaxFileSizeBytes = 10 * 1024 * 1024,
            AllowedExtensions = [".pdf", ".docx"]
        }));
    }

    [Fact]
    public void Validate_WithValidFile_HasNoErrors()
    {
        var command = new UploadAssignmentCommand
        {
            FileName = "instructions.pdf",
            FileSizeBytes = 1024
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDisallowedExtension_HasValidationError()
    {
        var command = new UploadAssignmentCommand
        {
            FileName = "script.exe",
            FileSizeBytes = 1024
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void Validate_WithZeroFileSize_HasValidationError()
    {
        var command = new UploadAssignmentCommand
        {
            FileName = "instructions.pdf",
            FileSizeBytes = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes);
    }
}
