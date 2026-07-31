using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features.Submissions.Commands.DeleteSubmission;
using ClassAssignmentSystem.Application.Features.Submissions.Commands.SubmitAssignment;
using ClassAssignmentSystem.Application.Features.Submissions.Queries.GetSubmissionDownloadUrl;
using ClassAssignmentSystem.Application.Features.Submissions.Queries.GetSubmissionsForAssignment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClassAssignmentSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ISender _sender;

    public SubmissionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Student uploads (or replaces, pre-due-date) their submission file for an assignment.
    /// Uses multipart/form-data so the file streams straight through to Azure Blob Storage
    /// without ASP.NET buffering the whole body in memory.
    /// </summary>
    [HttpPost("assignments/{assignmentId:int}")]
    [Authorize(Roles = "Student")]
    [RequestSizeLimit(15_000_000)] // slightly above MaxFileSizeBytes to allow for multipart overhead
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<SubmissionDto>> Submit(Guid assignmentId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file was uploaded.");

        await using var stream = file.OpenReadStream();

        var result = await _sender.Send(new SubmitAssignmentCommand
        {
            AssignmentId = assignmentId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            Content = stream
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>Returns a short-lived SAS URL the client uses to download the file directly from Azure.</summary>
    [HttpGet("{submissionId:int}/download-url")]
    public async Task<ActionResult<SubmissionDownloadUrlDto>> GetDownloadUrl(Guid submissionId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSubmissionDownloadUrlQuery { SubmissionId = submissionId }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Teacher/Admin: list all student submissions for an assignment.</summary>
    [HttpGet("assignments/{assignmentId:int}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<List<SubmissionDto>>> GetForAssignment(Guid assignmentId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSubmissionsForAssignmentQuery { AssignmentId = assignmentId }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Student deletes their own ungraded submission.</summary>
    [HttpDelete("{submissionId:int}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Delete(Guid submissionId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteSubmissionCommand { SubmissionId = submissionId }, cancellationToken);
        return NoContent();
    }
}
