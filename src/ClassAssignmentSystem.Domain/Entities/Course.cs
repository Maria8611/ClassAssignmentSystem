using ClassAssignmentSystem.Domain.Common;
using ClassAssignmentSystem.Domain.Enums;

namespace ClassAssignmentSystem.Domain.Entities;

public class Course : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int TotalSeats { get; private set; }
    public CourseStatus Status { get; private set; }

    public Guid? TeacherId { get; private set; }
    public User? Teacher { get; private set; }

    public ICollection<EnrollmentRequest> EnrollmentRequests { get; private set; } = new List<EnrollmentRequest>();

    private Course() { } // EF Core

    public static Course Create(string title, string description, int totalSeats)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        if (totalSeats <= 0) throw new ArgumentException("Total seats must be greater than zero.", nameof(totalSeats));

        return new Course
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description.Trim(),
            TotalSeats = totalSeats,
            Status = CourseStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AssignTeacher(Guid teacherId) => TeacherId = teacherId;

    public void RemoveTeacher() => TeacherId = null;

    public void Update(string title, string description, int totalSeats)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (totalSeats <= 0) throw new ArgumentException("Total seats must be greater than zero.", nameof(totalSeats));

        Title = title.Trim();
        Description = description.Trim();
        TotalSeats = totalSeats;
    }

    public void Deactivate() => Status = CourseStatus.Inactive;
    public void Activate() => Status = CourseStatus.Active;

    /// <summary>
    /// Domain rule: checks if a new enrollment request can be submitted.
    /// The actual approved count is passed in — avoids lazy-loading pitfalls.
    /// </summary>
    public bool HasAvailableSeats(int currentApprovedCount)
        => currentApprovedCount < TotalSeats;

    public int AvailableSeats(int currentApprovedCount)
        => Math.Max(0, TotalSeats - currentApprovedCount);
}