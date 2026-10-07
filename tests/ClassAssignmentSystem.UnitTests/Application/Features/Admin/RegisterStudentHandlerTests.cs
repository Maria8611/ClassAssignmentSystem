using ClassAssignmentSystem.Application.Features.Admin.Commands;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Repositories;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Admin;

public class RegisterStudentHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly RegisterStudentHandler _sut;

    public RegisterStudentHandlerTests()
    {
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _sut = new RegisterStudentHandler(_users.Object, _hasher.Object);
    }

    [Fact]
    public async Task Handle_WithNewEmail_CreatesStudent()
    {
        var dto = TestDataBuilder.CreateRegisterStudentDto();
        _users.Setup(r => r.EmailExistsAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.Handle(new RegisterStudentCommand(dto), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(dto.Email.ToLowerInvariant());
        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ReturnsConflictFailure()
    {
        var dto = TestDataBuilder.CreateRegisterStudentDto();
        _users.Setup(r => r.EmailExistsAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.Handle(new RegisterStudentCommand(dto), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Duplicate Email");
    }
}
