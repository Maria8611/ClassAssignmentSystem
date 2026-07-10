
namespace ClassAssignmentSystem.Application.DTOs
{
    public record CreateCourseDto(string Title, string Description, int TotalSeats);

    public record UpdateCourseDto(string Title, string Description, int TotalSeats);

    public record AssignTeacherDto(Guid TeacherId);

    public record CourseDto(
        Guid Id,
        string Title,
        string Description,
        int TotalSeats,
        int AvailableSeats,
        string Status,
        Guid? TeacherId,
        string? TeacherName,
        DateTime CreatedAt);
}
