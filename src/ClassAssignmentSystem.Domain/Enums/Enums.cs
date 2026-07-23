namespace ClassAssignmentSystem.Domain.Enums;

public enum UserRole
{
    Admin = 1,
    Teacher = 2,
    Student = 3
}

public enum EnrollmentStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum CourseStatus
{
    Active = 1,
    Inactive = 2
}
public enum SubmissionStatus
{
    Draft = 1,       // student saved but not yet confirmed
    Submitted = 2,   // confirmed — locked forever
}

public enum GradeStatus
{
    Pending = 1,     // submitted, not graded yet
    Graded = 2
}