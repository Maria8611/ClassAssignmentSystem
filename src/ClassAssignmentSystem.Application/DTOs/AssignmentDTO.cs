using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.DTOs
{
    public record CreateAssignmentDto(
        string Title,
        string Description,
        DateTime Deadline,
        int MaxMarks,
        Guid CourseId);

    public record UpdateAssignmentDto(
        string Title,
        string Description,
        DateTime Deadline,
        int MaxMarks);

    public record AssignmentDto(
        Guid Id,
        string Title,
        string Description,
        DateTime Deadline,
        int MaxMarks,
        Guid CourseId,
        string CourseTitle,
        Guid CreatedByTeacherId,
        string TeacherName,
        bool IsDeadlinePassed,
        DateTime CreatedAt);
}
  
   