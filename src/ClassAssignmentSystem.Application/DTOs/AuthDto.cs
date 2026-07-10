namespace ClassAssignmentSystem.Application.DTOs
{
public record LoginDto(string Email, string Password);

public record AuthResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    string Token);
}