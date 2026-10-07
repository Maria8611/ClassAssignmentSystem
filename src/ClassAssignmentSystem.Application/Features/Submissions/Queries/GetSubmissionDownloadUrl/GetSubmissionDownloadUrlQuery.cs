using ClassAssignmentSystem.Application.Common.Interfaces;
using ClassAssignmentSystem.Application.Configurations;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClassAssignmentSystem.Application.Features.Submissions.Queries.GetSubmissionDownloadUrl;

public record GetSubmissionDownloadUrlQuery : IRequest<SubmissionDownloadUrlDto>
{
    public Guid SubmissionId { get; init; }
}

public record SubmissionDownloadUrlDto(string FileName, string DownloadUrl, DateTime ExpiresAtUtc);

public class GetSubmissionDownloadUrlQueryHandler : IRequestHandler<GetSubmissionDownloadUrlQuery, SubmissionDownloadUrlDto>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly BlobStorageOptions _options;

    public GetSubmissionDownloadUrlQueryHandler(
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

    public async Task<SubmissionDownloadUrlDto> Handle(GetSubmissionDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var user = _currentUserService?.UserId;

        if (user is null)
            throw new ForbiddenException("An authenticated user is required.");

        var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId, cancellationToken) ?? throw new NotFoundException(nameof(Submission), request.SubmissionId);

        // A student may only download their own work; teachers/admins are authorized
        // at the controller level via [Authorize(Roles = "Teacher,Admin")] on the teacher route,
        // so this check specifically covers the student-facing route.
        

        var isOwner = submission.StudentId == _currentUserService?.UserId;
        var isPrivileged = _currentUserService.IsInRole("Teacher") || _currentUserService.IsInRole("Admin");

        if (!isOwner && !isPrivileged)
            throw new ForbiddenException("You do not have access to this submission.");
        
        if (string.IsNullOrWhiteSpace(submission.StoredFileName))
            throw new NotFoundException(nameof(Submission),"The submission has no stored file.");


        var expiry = TimeSpan.FromMinutes(_options.SasTokenExpiryMinutes);
        var url = await _blobStorageService.GetReadSasUriAsync(
            _options.SubmissionsContainer, submission.StoredFileName, expiry, cancellationToken);

        return new SubmissionDownloadUrlDto(submission.FileName, url, DateTime.UtcNow.Add(expiry));
    }
}
