using ClassAssignmentSystem.Application.Features.Enrollments;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Enrollments;

public class ApproveEnrollmentHandlerTests
{
    private readonly Mock<IEnrollmentRequestRepository> _enrollments = new();
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly ApproveEnrollmentHandler _sut;

    public ApproveEnrollmentHandlerTests()
    {
        _sut = new ApproveEnrollmentHandler(_enrollments.Object, _courses.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ApprovesEnrollment()
    {
        var teacherId = Guid.NewGuid();
        var course = TestDataBuilder.CreateActiveCourse(totalSeats: 5, teacherId: teacherId);
        var enrollment = TestDataBuilder.CreatePendingEnrollment(course.Id, Guid.NewGuid());
        _enrollments.Setup(r => r.GetByIdAsync(enrollment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(enrollment);
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _enrollments.Setup(r => r.GetApprovedCountByCourseAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await _sut.Handle(new ApproveEnrollmentCommand(teacherId, enrollment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Approved);
        _enrollments.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWrongTeacher_ReturnsForbiddenFailure()
    {
        var assignedTeacherId = Guid.NewGuid();
        var otherTeacherId = Guid.NewGuid();
        var course = TestDataBuilder.CreateActiveCourse(teacherId: assignedTeacherId);
        var enrollment = TestDataBuilder.CreatePendingEnrollment(course.Id, Guid.NewGuid());
        _enrollments.Setup(r => r.GetByIdAsync(enrollment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(enrollment);
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);

        var result = await _sut.Handle(new ApproveEnrollmentCommand(otherTeacherId, enrollment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ClassAssignmentSystem.Application.Common.Results.ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenCourseAtCapacity_ReturnsFailure()
    {
        var teacherId = Guid.NewGuid();
        var course = TestDataBuilder.CreateActiveCourse(totalSeats: 1, teacherId: teacherId);
        var enrollment = TestDataBuilder.CreatePendingEnrollment(course.Id, Guid.NewGuid());
        _enrollments.Setup(r => r.GetByIdAsync(enrollment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(enrollment);
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _enrollments.Setup(r => r.GetApprovedCountByCourseAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.Handle(new ApproveEnrollmentCommand(teacherId, enrollment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MaxLimitExceeded");
    }

    [Fact]
    public async Task Handle_WhenEnrollmentNotFound_ThrowsNotFoundException()
    {
        var enrollmentId = Guid.NewGuid();
        _enrollments.Setup(r => r.GetByIdAsync(enrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnrollmentRequest?)null);

        var act = () => _sut.Handle(new ApproveEnrollmentCommand(Guid.NewGuid(), enrollmentId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
