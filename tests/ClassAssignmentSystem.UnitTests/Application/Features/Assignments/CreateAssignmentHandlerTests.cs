using ClassAssignmentSystem.Application.Features.Assignments.Commands.CreateAssignment;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Assignments;

public class CreateAssignmentHandlerTests
{
    private readonly Mock<IAssignmentRepository> _assignments = new();
    private readonly Mock<ICourseRepository> _courses = new();

    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly CreateAssignmentHandler _sut;

    public CreateAssignmentHandlerTests()
    {
        _sut = new CreateAssignmentHandler(_assignments.Object, _courses.Object, _currentUser.Object);
    }

    [Fact]
    public async Task Handle_WithAssignedTeacher_CreatesAssignment()
    {
        //TODO

        //var teacherId = Guid.NewGuid();
        //var course = TestDataBuilder.CreateActiveCourse(teacherId: teacherId);
        //var dto = TestDataBuilder.CreateAssignmentDto(course.Id);
        //_courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);

        //var result = await _sut.Handle(new CreateAssignmentCommand(dto), CancellationToken.None);

        //result.IsSuccess.Should().BeTrue();
        //result.Value.Title.Should().Be(dto.Title);
        //_assignments.Verify(r => r.AddAsync(It.IsAny<Assignment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTeacherDoesNotOwnCourse_ReturnsForbiddenFailure()
    {
        var course = TestDataBuilder.CreateActiveCourse(teacherId: Guid.NewGuid());
        var dto = TestDataBuilder.CreateAssignmentDto(course.Id);
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);

        var result = await _sut.Handle(new CreateAssignmentCommand(dto), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Forbidden Access");
    }

    [Fact]
    public async Task Handle_WhenCourseNotFound_ThrowsNotFoundException()
    {
        var courseId = Guid.NewGuid();
        _courses.Setup(r => r.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        var act = () => _sut.Handle(
            new CreateAssignmentCommand(TestDataBuilder.CreateAssignmentDto(courseId)),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
