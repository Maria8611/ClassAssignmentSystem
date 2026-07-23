using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Entities;

namespace ClassAssignmentSystem.Application.Features;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user) => new(
        user.Id, user.FullName, user.Email, user.Role.ToString(), user.IsActive, user.CreatedAt);

    public static CourseDto ToDto(this Course course, int approvedCount) => new(
        course.Id, course.Title, course.Description, course.TotalSeats,
        course.AvailableSeats(approvedCount), course.Status.ToString(),
        course.TeacherId, course.Teacher?.FullName, course.CreatedAt);

    public static EnrollmentRequestDto ToDto(this EnrollmentRequest er, string courseTitle, string studentName) => new(
        er.Id, er.CourseId, courseTitle, er.StudentId, studentName,
        er.Status.ToString(), er.RejectionReason, er.CreatedAt, er.ActedAt);
    public static AssignmentDto ToDto(this Assignment assignment) => new(
        assignment.Id,assignment.Title, assignment.Description,assignment.Deadline, assignment.CourseId, assignment.MaxMarks, 
        assignment.CreatedByTeacherId, assignment.CreatedAt, assignment.Course.Title, assignment.Deadline > DateTime.UtcNow());
}
