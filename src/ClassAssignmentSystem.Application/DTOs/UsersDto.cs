
namespace ClassAssignmentSystem.Application.DTOs
{
    public record RegisterTeacherDto(string FullName, string Email, string Password);

    public record RegisterStudentDto(string FullName, string Email, string Password);

    public record UserDto(
        Guid Id,
        string FullName,
        string Email,
        string Role,
        bool IsActive,
        DateTime CreatedAt);
}
