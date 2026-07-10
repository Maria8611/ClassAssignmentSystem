using ClassAssignmentSystem.Domain.Common;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Exceptions;

namespace ClassAssignmentSystem.Domain.Entities;

public class EnrollmentRequest : BaseEntity
{
    public Guid CourseId { get; private set; }
    public Course Course { get; private set; } = null!;

    public Guid StudentId { get; private set; }
    public User Student { get; private set; } = null!;

    public EnrollmentStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? ActedAt { get; private set; }
    public Guid? ActedByTeacherId { get; private set; }

    private EnrollmentRequest() { } // EF Core

    public static EnrollmentRequest Create(Guid courseId, Guid studentId)
    {
        if (courseId == Guid.Empty) throw new ArgumentException("Course ID is required.", nameof(courseId));
        if (studentId == Guid.Empty) throw new ArgumentException("Student ID is required.", nameof(studentId));

        return new EnrollmentRequest
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            StudentId = studentId,
            Status = EnrollmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve(Guid teacherId)
    {
        EnsurePending();
        Status = EnrollmentStatus.Approved;
        ActedByTeacherId = teacherId;
        ActedAt = DateTime.UtcNow;
    }

    public void Reject(Guid teacherId, string? reason = null)
    {
        EnsurePending();
        Status = EnrollmentStatus.Rejected;
        RejectionReason = reason;
        ActedByTeacherId = teacherId;
        ActedAt = DateTime.UtcNow;
    }

    private void EnsurePending()
    {
        if (Status != EnrollmentStatus.Pending)
            throw new DomainException($"Cannot act on an enrollment request that is already '{Status}'.");
    }
}