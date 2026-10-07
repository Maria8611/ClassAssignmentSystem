using ClassAssignmentSystem.Application.Features.Admin.Commands;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Admin;

public class AssignTeacherHandlerTests
{
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly AssignTeacherHandler _sut;

    public AssignTeacherHandlerTests()
    {
        _sut = new AssignTeacherHandler(_courses.Object, _users.Object);
    }

    [Fact]
    public async Task Handle_WithValidTeacher_AssignsTeacherToCourse()
    {
        var course = TestDataBuilder.CreateActiveCourse();
        var teacher = TestDataBuilder.CreateTeacher();
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _users.Setup(r => r.GetByIdAsync(teacher.Id, It.IsAny<CancellationToken>())).ReturnsAsync(teacher);

        var result = await _sut.Handle(new AssignTeacherCommand(course.Id, teacher.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        course.TeacherId.Should().Be(teacher.Id);
        _courses.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotTeacher_ReturnsValidationFailure()
    {
        var course = TestDataBuilder.CreateActiveCourse();
        var student = TestDataBuilder.CreateStudent();
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _users.Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>())).ReturnsAsync(student);

        var result = await _sut.Handle(new AssignTeacherCommand(course.Id, student.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Failed Validation");
    }

    [Fact]
    public async Task Handle_WhenCourseNotFound_ThrowsNotFoundException()
    {
        var courseId = Guid.NewGuid();
        _courses.Setup(r => r.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        var act = () => _sut.Handle(new AssignTeacherCommand(courseId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
