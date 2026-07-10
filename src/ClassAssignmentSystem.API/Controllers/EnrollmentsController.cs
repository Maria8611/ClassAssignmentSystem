using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features.Courses;
using ClassAssignmentSystem.Application.Features.Enrollments;
using ClassAssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClassAssignmentSystem.API.Controllers;

[Authorize]
[Route("api/enrollments")]
public class EnrollmentsController : BaseApiController
{
    private readonly IMediator _mediator;
    public EnrollmentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Submit an enrollment request for a course. (Student only)</summary>
    [Authorize(Roles = "Student")]
    [HttpPost("courses/{courseId:guid}/request")]
    public async Task<IActionResult> RequestEnrollment(Guid courseId, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitEnrollmentRequestCommand(CurrentUserId, courseId), ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetMyEnrollments), result.Value);
    }

    /// <summary>Get the current student's enrollment requests. (Student only)</summary>
    [Authorize(Roles = "Student")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyEnrollments(CancellationToken ct)
        => Ok(await _mediator.Send(new GetMyEnrollmentsQuery(CurrentUserId), ct));

    /// <summary>Get all enrollment requests for the teacher's courses. Filter by ?status=Pending|Approved|Rejected (Teacher only)</summary>
    [Authorize(Roles = "Teacher")]
    [HttpGet("teacher")]
    public async Task<IActionResult> GetTeacherEnrollments([FromQuery] EnrollmentStatus? status, CancellationToken ct)
        => Ok(await _mediator.Send(new GetEnrollmentsByTeacherQuery(CurrentUserId, status), ct));

    /// <summary>Approve a student's enrollment request. (Teacher only)</summary>
    [Authorize(Roles = "Teacher")]
    [HttpPut("{requestId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid requestId, CancellationToken ct)
        => FromResult(await _mediator.Send(new ApproveEnrollmentCommand(CurrentUserId, requestId), ct));

    /// <summary>Reject a student's enrollment request with an optional reason. (Teacher only)</summary>
    [Authorize(Roles = "Teacher")]
    [HttpPut("{requestId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid requestId, [FromBody] RejectEnrollmentDto dto, CancellationToken ct)
        => FromResult(await _mediator.Send(new RejectEnrollmentCommand(CurrentUserId, requestId, dto.Reason), ct));
}
