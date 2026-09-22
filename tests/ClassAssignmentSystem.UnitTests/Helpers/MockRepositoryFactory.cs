using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Repositories;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Helpers;

public static class MockRepositoryFactory
{
    public static Mock<ICourseRepository> CreateCourseRepository(Course? course = null)
    {
        var mock = new Mock<ICourseRepository>();
        if (course is not null)
        {
            mock.Setup(r => r.GetByIdAsync(course.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(course);
        }
        return mock;
    }

    public static Mock<IEnrollmentRequestRepository> CreateEnrollmentRepository()
        => new();

    public static Mock<IUserRepository> CreateUserRepository(User? user = null)
    {
        var mock = new Mock<IUserRepository>();
        if (user is not null)
        {
            mock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            mock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
        }
        return mock;
    }

    public static Mock<IAssignmentRepository> CreateAssignmentRepository(Assignment? assignment = null)
    {
        var mock = new Mock<IAssignmentRepository>();
        if (assignment is not null)
        {
            mock.Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(assignment);
        }
        return mock;
    }

    public static Mock<ISubmissionRepository> CreateSubmissionRepository(Submission? submission = null)
    {
        var mock = new Mock<ISubmissionRepository>();
        if (submission is not null)
        {
            mock.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(submission);
        }
        return mock;
    }
}
