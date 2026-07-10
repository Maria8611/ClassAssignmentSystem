using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.DTOs
{
    public record EnrollmentRequestDto(
        Guid Id,
        Guid CourseId,
        string CourseTitle,
        Guid StudentId,
        string StudentName,
        string Status,
        string? RejectionReason,
        DateTime RequestedAt,
        DateTime? ActedAt);

    public record RejectEnrollmentDto(string? Reason);
}
