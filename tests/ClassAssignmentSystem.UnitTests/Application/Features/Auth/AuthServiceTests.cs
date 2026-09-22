using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features.Auth;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Repositories;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_users.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = TestDataBuilder.CreateStudent();
        _users.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(TestDataBuilder.DefaultPassword, user.PasswordHash))
            .Returns(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("jwt-token");

        var result = await _sut.LoginAsync(TestDataBuilder.CreateLoginDto(user.Email));

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token");
        result.Value.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ReturnsValidationFailure()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        var result = await _sut.LoginAsync(TestDataBuilder.CreateLoginDto("missing@test.com"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Invalid Credentials");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsValidationFailure()
    {
        var user = TestDataBuilder.CreateStudent();
        _users.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), user.PasswordHash))
            .Returns(false);

        var result = await _sut.LoginAsync(TestDataBuilder.CreateLoginDto(user.Email));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Invalid Credentials");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsForbiddenFailure()
    {
        var user = TestDataBuilder.CreateStudent();
        user.Deactivate();
        _users.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(TestDataBuilder.DefaultPassword, user.PasswordHash))
            .Returns(true);

        var result = await _sut.LoginAsync(TestDataBuilder.CreateLoginDto(user.Email));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
        result.Error.Code.Should().Be("Forbidden");
    }
}
