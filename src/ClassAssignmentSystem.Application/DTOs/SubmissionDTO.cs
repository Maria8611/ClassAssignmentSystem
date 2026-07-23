using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.DTOs
{
    public record SubmissionDto(
       Guid Id,
       Guid AssignmentId,
       string AssignmentTitle,
       Guid StudentId,
       string StudentName,
       string Status,           // Draft | Submitted
       string FileName,
       string ContentType,
       long FileSizeBytes,
       DateTime LastModifiedAt,
       DateTime? SubmittedAt,
       // Grading fields
       string GradeStatus,      // Pending | Graded
       int? MarksObtained,
       int MaxMarks,
       double? Percentage,
       string? TeacherComment,
       DateTime? GradedAt);

    public record GradeSubmissionDto(int MarksObtained, string? Comment);

    public record AddCommentDto(string Comment);

    public record ConfirmSubmissionDto(bool Confirmed); // client must send { confirmed: true }
}


