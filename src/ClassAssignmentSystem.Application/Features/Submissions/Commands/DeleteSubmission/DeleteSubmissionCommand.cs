using ClassAssignmentSystem.Application.Common.Interfaces;
using ClassAssignmentSystem.Application.Configurations;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClassAssignmentSystem.Application.Features.Submissions.Commands.DeleteSubmission;

public record DeleteSubmissionCommand : IRequest
{
    public Guid SubmissionId { get; init; }
}

public class DeleteSubmissionCommandHandler : IRequestHandler<DeleteSubmissionCommand>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly BlobStorageOptions _options;

    public DeleteSubmissionCommandHandler(
        ISubmissionRepository submissionRepository,
        IBlobStorageService blobStorageService,
        ICurrentUserService currentUserService,
        IOptions<BlobStorageOptions> options)
    {
        _submissionRepository = submissionRepository;
        _blobStorageService = blobStorageService;
        _currentUserService = currentUserService;
        _options = options.Value;
    }

    public async Task Handle(DeleteSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Submission), request.SubmissionId);

        if (submission.StudentId != _currentUserService.UserId)
            throw new ForbiddenException("You can only delete your own submission.");

        if (submission.Status == SubmissionStatus.Graded)
            throw new InvalidOperationException("A graded submission cannot be deleted.");

        await _blobStorageService.DeleteAsync(_options.SubmissionsContainer, submission.StoredFileName, cancellationToken);

        await _submissionRepository.DeleteAsync(submission);
        await _submissionRepository.SaveChangesAsync(cancellationToken);
    }
}
