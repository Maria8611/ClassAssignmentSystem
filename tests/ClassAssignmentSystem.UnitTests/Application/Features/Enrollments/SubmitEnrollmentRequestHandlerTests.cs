using ClassAssignmentSystem.Application.Features.Enrollments;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Enrollments;

public class SubmitEnrollmentRequestHandlerTests
{
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly Mock<IEnrollmentRequestRepository> _enrollments = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly SubmitEnrollmentRequestHandler _sut;

    public SubmitEnrollmentRequestHandlerTests()
    {
        _sut = new SubmitEnrollmentRequestHandler(_courses.Object, _enrollments.Object, _users.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesEnrollmentAndReturnsDto()
    {
        var student = TestDataBuilder.CreateStudent();
        var course = TestDataBuilder.CreateActiveCourse(teacherId: Guid.NewGuid());
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _enrollments.Setup(r => r.GetPendingByStudentAndCourseAsync(student.Id, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnrollmentRequest?)null);
        _enrollments.Setup(r => r.GetApprovedCountByCourseAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _users.Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>())).ReturnsAsync(student);

        var result = await _sut.Handle(new SubmitEnrollmentRequestCommand(student.Id, course.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CourseId.Should().Be(course.Id);
        _enrollments.Verify(r => r.AddAsync(It.IsAny<EnrollmentRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _enrollments.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCourseNotFound_ThrowsNotFoundException()
    {
        var courseId = Guid.NewGuid();
        _courses.Setup(r => r.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        var act = () => _sut.Handle(new SubmitEnrollmentRequestCommand(Guid.NewGuid(), courseId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCourseInactive_ReturnsValidationFailure()
    {
        var course = TestDataBuilder.CreateInactiveCourse();
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);

        var result = await _sut.Handle(new SubmitEnrollmentRequestCommand(Guid.NewGuid(), course.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InActive");
    }

    [Fact]
    public async Task Handle_WhenCourseHasNoTeacher_ReturnsValidationFailure()
    {
        var course = TestDataBuilder.CreateActiveCourse();
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);

        var result = await _sut.Handle(new SubmitEnrollmentRequestCommand(Guid.NewGuid(), course.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Unavailable");
    }

    [Fact]
    public async Task Handle_WhenDuplicatePendingRequest_ReturnsConflictFailure()
    {
        var studentId = Guid.NewGuid();
        var course = TestDataBuilder.CreateActiveCourse(teacherId: Guid.NewGuid());
        var pending = TestDataBuilder.CreatePendingEnrollment(course.Id, studentId);
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _enrollments.Setup(r => r.GetPendingByStudentAndCourseAsync(studentId, course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pending);

        var result = await _sut.Handle(new SubmitEnrollmentRequestCommand(studentId, course.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Duplicate Request");
    }

    [Fact]
    public async Task Handle_WhenCourseAtCapacity_ReturnsValidationFailure()
    {
        var course = TestDataBuilder.CreateActiveCourse(totalSeats: 1, teacherId: Guid.NewGuid());
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _enrollments.Setup(r => r.GetPendingByStudentAndCourseAsync(It.IsAny<Guid>(), course.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnrollmentRequest?)null);
        _enrollments.Setup(r => r.GetApprovedCountByCourseAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.Handle(new SubmitEnrollmentRequestCommand(Guid.NewGuid(), course.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MaxLimitExceeded");
    }
}
