using ClassAssignmentSystem.Application.Common.Interfaces;
using ClassAssignmentSystem.Application.Configurations;
using ClassAssignmentSystem.Application.Features.Submissions.Queries.GetSubmissionDownloadUrl;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using ClassAssignmentSystem.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace ClassAssignmentSystem.UnitTests.Application.Features.Submissions;

public class GetSubmissionDownloadUrlQueryHandlerTests
{
    private readonly Mock<ISubmissionRepository> _submissions = new();
    private readonly Mock<IBlobStorageService> _blobStorage = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly GetSubmissionDownloadUrlQueryHandler _sut;

    public GetSubmissionDownloadUrlQueryHandlerTests()
    {
        _sut = new GetSubmissionDownloadUrlQueryHandler(
            _submissions.Object,
            _blobStorage.Object,
            _currentUser.Object,
            Options.Create(new BlobStorageOptions
            {
                SubmissionsContainer = "submissions",
                SasTokenExpiryMinutes = 15
            }));
    }

    [Fact]
    public async Task Handle_WhenOwner_ReturnsDownloadUrl()
    {
        var studentId = Guid.NewGuid();
        var submission = TestDataBuilder.CreateDraftSubmission(Guid.NewGuid(), studentId);
        _submissions.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>())).ReturnsAsync(submission);
        _currentUser.Setup(c => c.UserId).Returns(studentId);
        _blobStorage.Setup(b => b.GetReadSasUriAsync("submissions", submission.StoredFileName, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://example.com/download");

        var result = await _sut.Handle(
            new GetSubmissionDownloadUrlQuery { SubmissionId = submission.Id },
            CancellationToken.None);

        result.DownloadUrl.Should().Be("https://example.com/download");
        result.FileName.Should().Be(submission.FileName);
    }

    [Fact]
    public async Task Handle_WhenTeacherRole_ReturnsDownloadUrl()
    {
        var submission = TestDataBuilder.CreateDraftSubmission(Guid.NewGuid(), Guid.NewGuid());
        _submissions.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>())).ReturnsAsync(submission);
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.IsInRole("Teacher")).Returns(true);
        _currentUser.Setup(c => c.IsInRole("Admin")).Returns(false);
        _blobStorage.Setup(b => b.GetReadSasUriAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://example.com/download");

        var result = await _sut.Handle(
            new GetSubmissionDownloadUrlQuery { SubmissionId = submission.Id },
            CancellationToken.None);

        result.DownloadUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WhenUnauthorizedUser_ThrowsForbiddenException()
    {
        var submission = TestDataBuilder.CreateDraftSubmission(Guid.NewGuid(), Guid.NewGuid());
        _submissions.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>())).ReturnsAsync(submission);
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.IsInRole(It.IsAny<string>())).Returns(false);

        var act = () => _sut.Handle(
            new GetSubmissionDownloadUrlQuery { SubmissionId = submission.Id },
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
