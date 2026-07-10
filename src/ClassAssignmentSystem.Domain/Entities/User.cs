using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Common;

namespace ClassAssignmentSystem.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    public ICollection<Course> TaughtCourses { get; private set; } = new List<Course>();
    public ICollection<EnrollmentRequest> EnrollmentRequests { get; private set; } = new List<EnrollmentRequest>();

    private User() { } // EF Core

    public static User CreateTeacher(string fullName, string email, string passwordHash)
        => Create(fullName, email, passwordHash, UserRole.Teacher);

    public static User CreateStudent(string fullName, string email, string passwordHash)
        => Create(fullName, email, passwordHash, UserRole.Student);

    public static User CreateAdmin(string fullName, string email, string passwordHash)
        => Create(fullName, email, passwordHash, UserRole.Admin);

    private static User Create(string fullName, string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void UpdateName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.", nameof(fullName));
        FullName = fullName.Trim();
    }
}