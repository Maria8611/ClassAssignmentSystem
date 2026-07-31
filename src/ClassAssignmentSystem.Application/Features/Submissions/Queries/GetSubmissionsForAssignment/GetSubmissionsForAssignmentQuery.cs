using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;

namespace ClassAssignmentSystem.Application.Features.Submissions.Queries.GetSubmissionsForAssignment;

public record GetSubmissionsForAssignmentQuery : IRequest<List<SubmissionDto>>
{
    public Guid AssignmentId { get; init; }
}

public class GetSubmissionsForAssignmentQueryHandler
    : IRequestHandler<GetSubmissionsForAssignmentQuery, List<SubmissionDto>>
{
    private readonly ISubmissionRepository _submissionRepository;

    public GetSubmissionsForAssignmentQueryHandler(ISubmissionRepository submissionRepository)
    {
        _submissionRepository = submissionRepository;
    }

    public async Task<List<SubmissionDto>> Handle(GetSubmissionsForAssignmentQuery request, CancellationToken cancellationToken)
    {
        // Authorization (Teacher owns the class / Admin) is enforced at the controller
        // via [Authorize(Roles = "Teacher,Admin")] plus a class-ownership check there,
        // matching the pattern already used by AssignmentsController.
        IReadOnlyList<Submission> res = await _submissionRepository.GetByAssignmentIdAsync(request.AssignmentId, cancellationToken);
        return res.Select(s => s.ToDto()).ToList();
    }
}
