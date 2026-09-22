using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ClassAssignmentSystem.Domain.UnitTests.Entities;

public class EnrollmentRequestTests
{
    private static readonly Guid CourseId = Guid.NewGuid();
    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly Guid TeacherId = Guid.NewGuid();

    #region Create

    [Fact]
    public void Create_WithValidIds_ReturnsPendingEnrollmentRequest()
    {
        var before = DateTime.UtcNow;

        var request = EnrollmentRequest.Create(CourseId, StudentId);

        var after = DateTime.UtcNow;

        request.Id.Should().NotBe(Guid.Empty);
        request.CourseId.Should().Be(CourseId);
        request.StudentId.Should().Be(StudentId);
        request.Status.Should().Be(EnrollmentStatus.Pending);
        request.RejectionReason.Should().BeNull();
        request.ActedAt.Should().BeNull();
        request.ActedByTeacherId.Should().BeNull();
        request.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_CalledTwice_GeneratesUniqueIds()
    {
        var first = EnrollmentRequest.Create(CourseId, StudentId);
        var second = EnrollmentRequest.Create(CourseId, StudentId);

        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Create_WithEmptyCourseId_ThrowsArgumentException()
    {
        var act = () => EnrollmentRequest.Create(Guid.Empty, StudentId);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("courseId");
    }

    [Fact]
    public void Create_WithEmptyStudentId_ThrowsArgumentException()
    {
        var act = () => EnrollmentRequest.Create(CourseId, Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("studentId");
    }

    #endregion

    #region Approve

    [Fact]
    public void Approve_WhenPending_SetsStatusApprovedAndRecordsActingTeacher()
    {
        var request = EnrollmentRequest.Create(CourseId, StudentId);
        var before = DateTime.UtcNow;

        request.Approve(TeacherId);

        var after = DateTime.UtcNow;

        request.Status.Should().Be(EnrollmentStatus.Approved);
        request.ActedByTeacherId.Should().Be(TeacherId);
        request.ActedAt.Should().NotBeNull();
        request.ActedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        request.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ThrowsDomainException()
    {
        var request = EnrollmentRequest.Create(CourseId, StudentId);
        request.Approve(TeacherId);

        var act = () => request.Approve(TeacherId);

        act.Should().Throw<DomainException>()
            .WithMessage("*already 'Approved'*");
    }

    [Fact]
    public void Approve_WhenAlreadyRejected_ThrowsDomainException()
    {
        var request = EnrollmentRequest.Create(CourseId, StudentId);
        request.Reject(TeacherId, "No seats available");

        var act = () => request.Approve(TeacherId);

        act.Should().Throw<DomainException>()
            .WithMessage("*already 'Rejected'*");
    }

    #endregion

    #region Reject

    [Fact]
    public void Reject_WhenPendingWithReason_SetsStatusRejectedAndStoresReason()
    {
        var request = EnrollmentRequest.Create(CourseId, StudentId);
        const string reason = "Course is full";
        var before = DateTime.UtcNow;

        request.Reject(TeacherId, reason);

        var after = DateTime.UtcNow;

        request.Status.Should().Be(EnrollmentStatus.Rejected);
        request.RejectionReason.Should().Be(reason);
        request.ActedByTeacherId.Should().Be(TeacherId);
        request.ActedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Reject_WhenPendingWithoutReason_SetsStatusRejectedWithNullReason()
    {
        var request = EnrollmentRequest.Create(CourseId, StudentId);

        request.Reject(TeacherId);

        request.Status.Should().Be(EnrollmentStatus.Rejected);
        request.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Reject_WhenAlreadyApproved_ThrowsDomainException()
    {
        var request = EnrollmentRequest.Create(CourseId, StudentId);
        request.Approve(TeacherId);

        var act = () => request.Reject(TeacherId, "changed my mind");

        act.Should().Throw<DomainException>()
            .WithMessage("*already 'Approved'*");
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ThrowsDomainException()
    {
        var request = EnrollmentRequest.Create(CourseId, StudentId);
        request.Reject(TeacherId, "First reason");

        var act = () => request.Reject(TeacherId, "Second reason");

        act.Should().Throw<DomainException>()
            .WithMessage("*already 'Rejected'*");
    }

    #endregion
}