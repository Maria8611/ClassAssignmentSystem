using ClassAssignmentSystem.Application.Features.Enrollments;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Repositories;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Enrollments;

public class RejectEnrollmentHandlerTests
{
    private readonly Mock<IEnrollmentRequestRepository> _enrollments = new();
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly RejectEnrollmentHandler _sut;

    public RejectEnrollmentHandlerTests()
    {
        _sut = new RejectEnrollmentHandler(_enrollments.Object, _courses.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_RejectsEnrollment()
    {
        var teacherId = Guid.NewGuid();
        var course = TestDataBuilder.CreateActiveCourse(teacherId: teacherId);
        var enrollment = TestDataBuilder.CreatePendingEnrollment(course.Id, Guid.NewGuid());
        _enrollments.Setup(r => r.GetByIdAsync(enrollment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(enrollment);
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);

        var result = await _sut.Handle(
            new RejectEnrollmentCommand(teacherId, enrollment.Id, "No seats"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        enrollment.Status.Should().Be(EnrollmentStatus.Rejected);
        enrollment.RejectionReason.Should().Be("No seats");
    }

    [Fact]
    public async Task Handle_WhenWrongTeacher_ReturnsForbiddenFailure()
    {
        var course = TestDataBuilder.CreateActiveCourse(teacherId: Guid.NewGuid());
        var enrollment = TestDataBuilder.CreatePendingEnrollment(course.Id, Guid.NewGuid());
        _enrollments.Setup(r => r.GetByIdAsync(enrollment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(enrollment);
        _courses.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>())).ReturnsAsync(course);

        var result = await _sut.Handle(
            new RejectEnrollmentCommand(Guid.NewGuid(), enrollment.Id, "reason"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
