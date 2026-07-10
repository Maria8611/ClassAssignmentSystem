using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features.Admin;
using ClassAssignmentSystem.Application.Features.Admin.Commands;
using ClassAssignmentSystem.Application.Features.Admin.Queries;
using ClassAssignmentSystem.Application.Features.Courses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClassAssignmentSystem.API.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : BaseApiController
{
    private readonly IMediator _mediator;
    public AdminController(IMediator mediator) => _mediator = mediator;

    /// <summary>Register a new teacher account.</summary>
    [HttpPost("teachers")]
    public async Task<IActionResult> RegisterTeacher([FromBody] RegisterTeacherDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterTeacherCommand(dto), ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetTeachers), result.Value);
    }

    /// <summary>Get all teacher accounts.</summary>
    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers(CancellationToken ct)
        => Ok(await _mediator.Send(new GetTeachersQuery(), ct));

    /// <summary>Register a new student account.</summary>
    [HttpPost("students")]
    public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterStudentCommand(dto), ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetStudents), result.Value);
    }

    /// <summary>Get all student accounts.</summary>
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(CancellationToken ct)
        => Ok(await _mediator.Send(new GetStudentsQuery(), ct));

    /// <summary>Create a new course.</summary>
    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCourseCommand(dto), ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetCourse), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Get a specific course by ID.</summary>
    [HttpGet("courses/{id:guid}")]
    public async Task<IActionResult> GetCourse(Guid id, CancellationToken ct)
    {
        var course = await _mediator.Send(new GetCourseByIdQuery(id), ct);
        return course is null ? NotFound() : Ok(course);
    }

    /// <summary>Update an existing course.</summary>
    [HttpPut("courses/{id:guid}")]
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDto dto, CancellationToken ct)
        => FromResult(await _mediator.Send(new UpdateCourseCommand(id, dto), ct));

    /// <summary>Assign a teacher to a course.</summary>
    [HttpPut("courses/{courseId:guid}/teacher")]
    public async Task<IActionResult> AssignTeacher(Guid courseId, [FromBody] AssignTeacherDto dto, CancellationToken ct)
        => FromResult(await _mediator.Send(new AssignTeacherCommand(courseId, dto.TeacherId), ct));
}
