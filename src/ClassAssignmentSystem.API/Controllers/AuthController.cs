using System.Security.Claims;
using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClassAssignmentSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));

    protected IActionResult FromResult(Result result)
        => result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });

    protected IActionResult FromResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
}

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Login and receive a JWT token.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        => FromResult(await _authService.LoginAsync(dto, ct));
}
