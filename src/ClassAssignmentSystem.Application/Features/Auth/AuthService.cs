using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Repositories;

namespace ClassAssignmentSystem.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public AuthService(IUserRepository users, IPasswordHasher hasher, IJwtService jwt)
    {
        _users = users; _hasher = hasher; _jwt = jwt;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(dto.Email.ToLowerInvariant(), ct);

        if (user is null || !_hasher.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Failure(Error.Validation("Invalid Credentials","Invalid email or password."));

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure(Error.Forbidden("Forbidden","Your account has been deactivated. Please contact an admin."));

        var token = _jwt.GenerateToken(user);
        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            user.Id, user.FullName, user.Email, user.Role.ToString(), token));
    }
}
