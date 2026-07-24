using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features.Assignments.Commands.CreateAssignment;
using ClassAssignmentSystem.Application.Features.Courses;
using ClassAssignmentSystem.Application.Features.Enrollments;
using ClassAssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClassAssignmentSystem.API.Controllers
{
    [Authorize]
    [Route("api/Assignment")]
    public class AssignmentsController : BaseApiController
    {
        private readonly IMediator _mediator;
        public AssignmentsController(IMediator mediator) => _mediator = mediator;

        /// <summary>Create a new assignment</summary>
        [Authorize(Roles = "Teacher")]
        [HttpPost("Assignment/{TeacherId:guid}")]
        public async Task<IActionResult> CreateAssignment(Guid TeacherId, CreateAssignmentDto Dto, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateAssignmentCommand(TeacherId, Dto), ct);
            if (result.IsFailure) return BadRequest(new { error = result.Error });
            return CreatedAtAction(nameof(GetAssignment), new { id = result.Value!.Id }, result.Value);
        }

        /// <summary>Get a specific assignment by ID.</summary>
        [HttpGet("assignment/{id:guid}")]
        public async Task<IActionResult> GetAssignment(Guid id, CancellationToken ct)
        {
            var course = await _mediator.Send(new GetAssignmentByIdQuery(id), ct);
            return course is null ? NotFound() : Ok(course);
        }
        }

}
