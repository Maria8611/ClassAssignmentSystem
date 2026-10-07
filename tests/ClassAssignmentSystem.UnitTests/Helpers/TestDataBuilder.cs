using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;

namespace ClassAssignmentSystem.UnitTests.Helpers;

public static class TestDataBuilder
{
    public const string DefaultPassword = "Password123!";
    public const string DefaultPasswordHash = "hashed-password";

    public static User CreateTeacher(
        string fullName = "Jane Teacher",
        string email = "teacher@test.com",
        string passwordHash = DefaultPasswordHash)
        => User.CreateTeacher(fullName, email, passwordHash);

    public static User CreateStudent(
        string fullName = "John Student",
        string email = "student@test.com",
        string passwordHash = DefaultPasswordHash)
        => User.CreateStudent(fullName, email, passwordHash);

    public static User CreateAdmin(
        string fullName = "Admin User",
        string email = "admin@test.com",
        string passwordHash = DefaultPasswordHash)
        => User.CreateAdmin(fullName, email, passwordHash);

    public static Course CreateActiveCourse(
        string title = "Test Course",
        string description = "Test course description",
        int totalSeats = 30,
        Guid? teacherId = null)
    {
        var course = Course.Create(title, description, totalSeats);
        if (teacherId.HasValue)
            course.AssignTeacher(teacherId.Value);
        return course;
    }

    public static Course CreateInactiveCourse(int totalSeats = 30)
    {
        var course = CreateActiveCourse(totalSeats: totalSeats);
        course.Deactivate();
        return course;
    }

    public static EnrollmentRequest CreatePendingEnrollment(Guid courseId, Guid studentId)
        => EnrollmentRequest.Create(courseId, studentId);

    public static Assignment CreateAssignment(
        Guid courseId,
        Guid teacherId,
        DateTime? deadline = null,
        int maxMarks = 100)
        => Assignment.Create(
            "Test Assignment",
            "Assignment description",
            deadline ?? DateTime.UtcNow.AddDays(7),
            maxMarks,
            courseId,
            teacherId);

    public static Submission CreateDraftSubmission(
        Guid assignmentId,
        Guid studentId,
        string fileName = "report.pdf",
        string storedFileName = "blob/report.pdf")
        => Submission.CreateDraft(
            assignmentId,
            studentId,
            fileName,
            storedFileName,
            "application/pdf",
            1024);

    public static RegisterTeacherDto CreateRegisterTeacherDto(
        string fullName = "Jane Teacher",
        string email = "teacher@test.com",
        string password = DefaultPassword)
        => new(fullName, email, password);

    public static RegisterStudentDto CreateRegisterStudentDto(
        string fullName = "John Student",
        string email = "student@test.com",
        string password = DefaultPassword)
        => new(fullName, email, password);

    public static LoginDto CreateLoginDto(
        string email = "student@test.com",
        string password = DefaultPassword)
        => new(email, password);

    public static CreateAssignmentDto CreateAssignmentDto(
        Guid courseId,
        DateTime? deadline = null,
        int maxMarks = 100)
        => new(
            "Test Assignment",
            "Assignment description",
            deadline ?? DateTime.UtcNow.AddDays(7),
            maxMarks,
            courseId);
}
