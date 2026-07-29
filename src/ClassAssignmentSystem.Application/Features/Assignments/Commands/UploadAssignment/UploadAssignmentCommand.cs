using ClassAssignmentSystem.Application.Common.Interfaces;
using ClassAssignmentSystem.Application.Configurations;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClassAssignmentSystem.Application.Assignments.Commands.UploadAssignment;

/// <summary>
/// A teacher attaches a reference/material file (e.g. instructions, a rubric,
/// starter code) to an assignment, stored in a separate container from student
/// submissions so the two never share a namespace or SAS scope.
/// </summary>
public record UploadAssignmentCommand : IRequest<string>
{
    public Guid AssignmentId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public Stream Content { get; init; } = Stream.Null;
}



public class UploadAssignmentCommandHandler : IRequestHandler<UploadAssignmentCommand, string>
{
    private readonly IAssignmentRepository _assignments;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ICurrentUserService _currentUser;
    private readonly BlobStorageOptions _options;

    public UploadAssignmentCommandHandler(
        IAssignmentRepository assignment,
        IBlobStorageService blobStorageService,
        ICurrentUserService currentUser,
        IOptions<BlobStorageOptions> options)
    {
        _assignments = assignment;
        _blobStorageService = blobStorageService;
        _currentUser = currentUser;
        _options = options.Value;
    }

    public async Task<string> Handle(UploadAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignments.GetByIdAsync(request.AssignmentId, cancellationToken)
                         ?? throw new NotFoundException(nameof(Assignment), request.AssignmentId);

        if (assignment.CreatedByTeacherId != _currentUser.UserId && !_currentUser.IsInRole("Admin"))
            throw new ForbiddenException("Only the owning teacher or an admin can attach materials to this assignment.");

        var safeFileName = Path.GetFileName(request.FileName);
        var blobName = $"{request.AssignmentId}/{Guid.NewGuid()}-{safeFileName}";

        await _blobStorageService.UploadAsync(
            _options.AssignmentMaterialsContainer,
            blobName,
            request.Content,
            request.ContentType,
            cancellationToken);

        assignment.UploadAssignmentMaterial(request.FileName, blobName,request.ContentType,request.FileSizeBytes
            ); 
        await _assignments.SaveChangesAsync(cancellationToken);

        return blobName;
    }
}
