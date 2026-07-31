using ClassAssignmentSystem.Application.Common.Interfaces;
using ClassAssignmentSystem.Application.Configurations;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClassAssignmentSystem.Application.Features.Submissions.Commands.SubmitAssignment;

/// <summary>
/// A student submits (or re-submits, if allowed and before the due date) a file
/// against an assignment. The file content is streamed straight to Azure Blob
/// Storage — it is never buffered fully in memory or written to local disk.
/// </summary>
public record SubmitAssignmentCommand : IRequest<SubmissionDto>
{
    public Guid AssignmentId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public Stream Content { get; init; } = Stream.Null;
}

public class SubmitAssignmentCommandValidator : AbstractValidator<SubmitAssignmentCommand>
{
    public SubmitAssignmentCommandValidator(IOptions<BlobStorageOptions> options)
    {
        var opts = options.Value;

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(name => opts.AllowedExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
            .WithMessage($"File type not allowed. Allowed types: {string.Join(", ", options.Value.AllowedExtensions)}");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(opts.MaxFileSizeBytes)
            .WithMessage($"File exceeds the maximum allowed size of {options.Value.MaxFileSizeBytes / (1024 * 1024)} MB.");
    }
}

public class SubmitAssignmentCommandHandler : IRequestHandler<SubmitAssignmentCommand, SubmissionDto>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IEnrollmentRequestRepository _enrollmentRequestRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly BlobStorageOptions _options;

    public SubmitAssignmentCommandHandler(
        ISubmissionRepository submissionRepository,
        IAssignmentRepository assignmentRepository,
        IEnrollmentRequestRepository enrollmentRequestRepository,
        IBlobStorageService blobStorageService,
        ICurrentUserService currentUserService,
        IOptions<BlobStorageOptions> options)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository; 
        _enrollmentRequestRepository = enrollmentRequestRepository;
        _blobStorageService = blobStorageService;
        _currentUserService = currentUserService;
        _options = options.Value;
    }

    public async Task<SubmissionDto> Handle(SubmitAssignmentCommand request, CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.UserId?? throw new UnauthorizedAccessException("No authenticated user found.");

        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Assignment), request.AssignmentId);

        // Only enrolled students for the assignment's class may submit.
        var isEnrolled = await _enrollmentRequestRepository.GetApprovedByStudentAndCourseAsync(studentId, assignment.CourseId, cancellationToken);

        if (isEnrolled is null)
            throw new ForbiddenException("You are not enrolled in the class this assignment belongs to.");

        var existingSubmission = await _submissionRepository.GetByAssignmentAndStudentAsync(request.AssignmentId, studentId, cancellationToken);

        var isLate = assignment.IsDeadlinePassed();
        if (isLate)
            throw new ValidationException("The due date has passed and late submissions are not allowed for this assignment.");

        if (existingSubmission is not null && DateTime.UtcNow > assignment.Deadline)
            throw new ValidationException("You cannot resubmit after the due date.");

        // Blob naming: submissions/{assignmentId}/{studentId}/{guid}-{originalFileName}
        // The GUID prefix avoids collisions and prevents guessing another student's file path.
        var safeFileName = Path.GetFileName(request.FileName);
        var blobName = $"{request.AssignmentId}/{studentId}/{Guid.NewGuid()}-{safeFileName}";

        await _blobStorageService.UploadAsync(
            _options.SubmissionsContainer,
            blobName,
            request.Content,
            request.ContentType,
            cancellationToken);

        if (existingSubmission is not null)
        {
            // Clean up the previously uploaded blob before pointing to the new one.
            await _blobStorageService.DeleteAsync(_options.SubmissionsContainer, existingSubmission.StoredFileName, cancellationToken);
            existingSubmission.ReplaceFile(safeFileName, blobName, request.ContentType, request.FileSizeBytes, isLate);
        }
        else
        {
            existingSubmission = Submission.CreateDraft(
                request.AssignmentId, studentId, safeFileName, blobName,
                request.ContentType, request.FileSizeBytes);

            await _submissionRepository.AddAsync(existingSubmission);
        }

        await _submissionRepository.SaveChangesAsync(cancellationToken);

        return existingSubmission.ToDto();
    }
}
