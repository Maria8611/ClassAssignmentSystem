using ClassAssignmentSystem.Application.Common.Interfaces;
using ClassAssignmentSystem.Application.Configurations;
using ClassAssignmentSystem.Application.Features.Submissions.Commands.DeleteSubmission;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Submissions;

public class DeleteSubmissionCommandHandlerTests
{
    private readonly Mock<ISubmissionRepository> _submissions = new();
    private readonly Mock<IBlobStorageService> _blobStorage = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly DeleteSubmissionCommandHandler _sut;

    public DeleteSubmissionCommandHandlerTests()
    {
        _sut = new DeleteSubmissionCommandHandler(
            _submissions.Object,
            _blobStorage.Object,
            _currentUser.Object,
            Options.Create(new BlobStorageOptions { SubmissionsContainer = "submissions" }));
    }

    [Fact]
    public async Task Handle_WithOwnDraftSubmission_DeletesBlobAndSubmission()
    {
        var studentId = Guid.NewGuid();
        var submission = TestDataBuilder.CreateDraftSubmission(Guid.NewGuid(), studentId);
        _submissions.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>())).ReturnsAsync(submission);
        _currentUser.Setup(c => c.UserId).Returns(studentId);

        await _sut.Handle(new DeleteSubmissionCommand { SubmissionId = submission.Id }, CancellationToken.None);

        _blobStorage.Verify(b => b.DeleteAsync("submissions", submission.StoredFileName, It.IsAny<CancellationToken>()), Times.Once);
        _submissions.Verify(r => r.Delete(submission), Times.Once);
        _submissions.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotOwner_ThrowsForbiddenException()
    {
        var submission = TestDataBuilder.CreateDraftSubmission(Guid.NewGuid(), Guid.NewGuid());
        _submissions.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>())).ReturnsAsync(submission);
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());

        var act = () => _sut.Handle(new DeleteSubmissionCommand { SubmissionId = submission.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenGraded_ThrowsInvalidOperationException()
    {
        var studentId = Guid.NewGuid();
        var submission = TestDataBuilder.CreateDraftSubmission(Guid.NewGuid(), studentId);
        submission.GetType().GetProperty(nameof(submission.Status))!
            .SetValue(submission, SubmissionStatus.Graded);
        _submissions.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>())).ReturnsAsync(submission);
        _currentUser.Setup(c => c.UserId).Returns(studentId);

        var act = () => _sut.Handle(new DeleteSubmissionCommand { SubmissionId = submission.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
