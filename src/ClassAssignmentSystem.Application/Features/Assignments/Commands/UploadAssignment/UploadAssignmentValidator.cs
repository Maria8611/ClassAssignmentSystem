using ClassAssignmentSystem.Application.Assignments.Commands.UploadAssignment;
using FluentValidation;
using Microsoft.Extensions.Options;
using ClassAssignmentSystem.Application.Configurations;

namespace ClassAssignmentSystem.Application.Features.Assignments.Commands.UploadAssignment
{
    public class UploadAssignmentCommandValidator : AbstractValidator<UploadAssignmentCommand>
    {
        public UploadAssignmentCommandValidator(IOptions<BlobStorageOptions> options)
        {
            var opts = options.Value;

            RuleFor(x => x.FileName)
                .NotEmpty()
                .Must(name => opts.AllowedExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
                .WithMessage($"File type not allowed. Allowed types: {string.Join(", ", opts.AllowedExtensions)}");
            RuleFor(x => x.FileSizeBytes).GreaterThan(0).LessThanOrEqualTo(opts.MaxFileSizeBytes);
        }
    }
}
