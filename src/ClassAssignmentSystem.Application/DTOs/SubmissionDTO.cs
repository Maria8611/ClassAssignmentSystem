using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;


namespace ClassAssignmentSystem.Application.DTOs
{
    public record SubmissionDto(
       Guid Id,
       Guid AssignmentId,
       string AssignmentTitle,
       Guid StudentId,
       string StudentName,
       SubmissionStatus Status,           // Draft | Submitted
       string FileName,
       string ContentType,
       long FileSizeBytes,
       DateTime LastModifiedAt,
       DateTime? SubmittedAt,
       GradeStatus GradeStatus,      
       int? MarksObtained,
       int MaxMarks,
       DateTime? GradedAt,
      string? Feedback);

    public record GradeSubmissionDto(int MarksObtained, string? Comment);

    public record AddCommentDto(string Comment);

    public record ConfirmSubmissionDto(bool Confirmed); // client must send { confirmed: true }
}


