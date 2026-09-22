using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ClassAssignmentSystem.Domain.UnitTests.Entities;

public class SubmissionTests
{
    private const string ValidFileName = "solution.pdf";
    private const string ValidStoredFileName = "blob-guid-name";
    private const string ValidContentType = "application/pdf";
    private const long ValidFileSizeBytes = 51_200;

    private static readonly Guid AssignmentId = Guid.NewGuid();
    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly Guid TeacherId = Guid.NewGuid();

    private static Submission CreateDraftSubmission(
        string fileName = ValidFileName,
        string storedFileName = ValidStoredFileName,
        string contentType = ValidContentType,
        long fileSizeBytes = ValidFileSizeBytes)
        => Submission.CreateDraft(AssignmentId, StudentId, fileName, storedFileName, contentType, fileSizeBytes);

    private static Submission CreateConfirmedSubmission()
    {
        var submission = CreateDraftSubmission();
        submission.Confirm();
        return submission;
    }

    #region CreateDraft

    [Fact]
    public void CreateDraft_WithValidInputs_SetsExpectedFieldsAndDraftState()
    {
        var before = DateTime.UtcNow;

        var submission = Submission.CreateDraft(
            AssignmentId, StudentId, ValidFileName, ValidStoredFileName, ValidContentType, ValidFileSizeBytes);

        var after = DateTime.UtcNow;

        submission.Id.Should().NotBe(Guid.Empty);
        submission.AssignmentId.Should().Be(AssignmentId);
        submission.StudentId.Should().Be(StudentId);
        submission.Status.Should().Be(SubmissionStatus.Draft);
        submission.FileName.Should().Be(ValidFileName);
        submission.StoredFileName.Should().Be(ValidStoredFileName);
        submission.ContentType.Should().Be(ValidContentType);
        submission.FileSizeBytes.Should().Be(ValidFileSizeBytes);
        submission.GradeStatus.Should().Be(GradeStatus.Pending);
        submission.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        submission.LastModifiedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        submission.SubmittedAt.Should().BeNull();
        submission.MarksObtained.Should().BeNull();
        submission.Feedback.Should().BeNull();
        submission.GradedByTeacherId.Should().BeNull();
        submission.GradedAt.Should().BeNull();
    }

    [Fact]
    public void CreateDraft_CalledTwice_GeneratesUniqueIds()
    {
        var first = CreateDraftSubmission();
        var second = CreateDraftSubmission();

        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void CreateDraft_WithEmptyAssignmentOrStudentId_DoesNotThrow()
    {
        // Documents current behavior: unlike EnrollmentRequest.Create, there is no
        // Guid.Empty guard on assignmentId/studentId here, so this is accepted as-is.
        var act = () => Submission.CreateDraft(
            Guid.Empty, Guid.Empty, ValidFileName, ValidStoredFileName, ValidContentType, ValidFileSizeBytes);

        act.Should().NotThrow();
    }

    [Fact]
    public void CreateDraft_WithDisallowedExtension_ThrowsDomainException()
    {
        var act = () => CreateDraftSubmission(fileName: "malware.exe");

        act.Should().Throw<DomainException>()
            .WithMessage("File type '.exe' is not allowed. Accepted: PDF, Word, PowerPoint, Excel, and common image formats.");
    }

    [Fact]
    public void CreateDraft_WithAllowedExtensionButDisallowedContentType_ThrowsDomainException()
    {
        var act = () => CreateDraftSubmission(contentType: "application/octet-stream");

        act.Should().Throw<DomainException>()
            .WithMessage("Content type 'application/octet-stream' is not permitted.");
    }

    [Fact]
    public void CreateDraft_ExtensionAndContentTypeMatchingIsCaseInsensitive()
    {
        var act = () => CreateDraftSubmission(fileName: "Photo.JPG", contentType: "IMAGE/JPEG");

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("notes.pdf", "application/pdf")]
    [InlineData("notes.doc", "application/msword")]
    [InlineData("notes.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("notes.ppt", "application/vnd.ms-powerpoint")]
    [InlineData("notes.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData("notes.xls", "application/vnd.ms-excel")]
    [InlineData("notes.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("notes.jpg", "image/jpeg")]
    [InlineData("notes.jpeg", "image/jpeg")]
    [InlineData("notes.png", "image/png")]
    [InlineData("notes.gif", "image/gif")]
    [InlineData("notes.webp", "image/webp")]
    public void CreateDraft_WithAllowedExtensionContentTypePairs_DoesNotThrow(string fileName, string contentType)
    {
        var act = () => CreateDraftSubmission(fileName: fileName, contentType: contentType);

        act.Should().NotThrow();
    }

    #endregion

    #region UpdateDraftFile

    [Fact]
    public void UpdateDraftFile_WhileDraft_ReplacesFileFieldsAndUpdatesTimestamp()
    {
        var submission = CreateDraftSubmission();
        var before = DateTime.UtcNow;

        submission.UpdateDraftFile("revised.docx", "blob-456",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 99_000);

        var after = DateTime.UtcNow;

        submission.FileName.Should().Be("revised.docx");
        submission.StoredFileName.Should().Be("blob-456");
        submission.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        submission.FileSizeBytes.Should().Be(99_000);
        submission.LastModifiedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void UpdateDraftFile_AfterConfirm_ThrowsDomainException()
    {
        var submission = CreateConfirmedSubmission();

        var act = () => submission.UpdateDraftFile(
            ValidFileName, ValidStoredFileName, ValidContentType, ValidFileSizeBytes);

        act.Should().Throw<DomainException>()
            .WithMessage("This submission has been confirmed and can no longer be modified.");
    }

    [Fact]
    public void UpdateDraftFile_OnConfirmedSubmission_ChecksStatusBeforeFileValidity()
    {
        var submission = CreateConfirmedSubmission();

        // File is also invalid here, but the "already confirmed" check should win —
        // proves EnsureDraft runs before ValidateFile, not after.
        var act = () => submission.UpdateDraftFile("malware.exe", "blob", "application/octet-stream", -1);

        act.Should().Throw<DomainException>()
            .WithMessage("This submission has been confirmed and can no longer be modified.");
    }

    [Fact]
    public void UpdateDraftFile_WithDisallowedExtension_WhileDraft_ThrowsDomainException()
    {
        var submission = CreateDraftSubmission();

        var act = () => submission.UpdateDraftFile("malware.exe", "blob", ValidContentType, ValidFileSizeBytes);

        act.Should().Throw<DomainException>()
            .WithMessage("File type '.exe' is not allowed. Accepted: PDF, Word, PowerPoint, Excel, and common image formats.");
    }

    #endregion

    #region Confirm

    [Fact]
    public void Confirm_FromDraft_SetsSubmittedStatusAndTimestamps()
    {
        var submission = CreateDraftSubmission();
        var before = DateTime.UtcNow;

        submission.Confirm();

        var after = DateTime.UtcNow;

        submission.Status.Should().Be(SubmissionStatus.Submitted);
        submission.SubmittedAt.Should().NotBeNull();
        submission.SubmittedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        submission.LastModifiedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ThrowsDomainException()
    {
        var submission = CreateConfirmedSubmission();

        var act = () => submission.Confirm();

        act.Should().Throw<DomainException>()
            .WithMessage("This submission has been confirmed and can no longer be modified.");
    }

    #endregion

    #region Grade

    [Fact]
    public void Grade_WhenStillDraft_ThrowsDomainException()
    {
        var submission = CreateDraftSubmission();

        var act = () => submission.Grade(80, 100, "Good work", TeacherId);

        act.Should().Throw<DomainException>()
            .WithMessage("You can only grade or comment on a confirmed submission.");
    }

    [Fact]
    public void Grade_WhenSubmitted_WithValidInputs_SetsGradeFieldsAndTrimsComment()
    {
        var submission = CreateConfirmedSubmission();
        var before = DateTime.UtcNow;

        submission.Grade(80, 100, "  Good work  ", TeacherId);

        var after = DateTime.UtcNow;

        submission.MarksObtained.Should().Be(80);
        submission.Feedback.Should().Be("Good work");
        submission.GradeStatus.Should().Be(GradeStatus.Graded);
        submission.GradedByTeacherId.Should().Be(TeacherId);
        submission.GradedAt.Should().NotBeNull();
        submission.GradedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        submission.LastModifiedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Grade_WithNullComment_SetsFeedbackToNull()
    {
        var submission = CreateConfirmedSubmission();

        submission.Grade(80, 100, null, TeacherId);

        submission.Feedback.Should().BeNull();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Grade_WithNegativeMarks_ThrowsDomainException(int marksObtained)
    {
        var submission = CreateConfirmedSubmission();

        var act = () => submission.Grade(marksObtained, 100, "comment", TeacherId);

        act.Should().Throw<DomainException>().WithMessage("Marks cannot be negative.");
    }

    [Fact]
    public void Grade_WithMarksExceedingMaxMarks_ThrowsDomainException()
    {
        var submission = CreateConfirmedSubmission();

        var act = () => submission.Grade(150, 100, "comment", TeacherId);

        act.Should().Throw<DomainException>().WithMessage("Marks cannot exceed max marks (100).");
    }

    [Fact]
    public void Grade_WithMarksEqualToMaxMarks_DoesNotThrow()
    {
        var submission = CreateConfirmedSubmission();

        var act = () => submission.Grade(100, 100, "Perfect score", TeacherId);

        act.Should().NotThrow();
        submission.MarksObtained.Should().Be(100);
    }

    [Fact]
    public void Grade_WithZeroMarks_DoesNotThrow()
    {
        var submission = CreateConfirmedSubmission();

        var act = () => submission.Grade(0, 100, "No credit", TeacherId);

        act.Should().NotThrow();
        submission.MarksObtained.Should().Be(0);
    }

    [Fact]
    public void Grade_CalledAgain_OverwritesPreviousGrade()
    {
        var submission = CreateConfirmedSubmission();
        submission.Grade(60, 100, "First pass", TeacherId);

        var secondTeacherId = Guid.NewGuid();
        submission.Grade(90, 100, "Regraded, missed a section earlier", secondTeacherId);

        submission.MarksObtained.Should().Be(90);
        submission.Feedback.Should().Be("Regraded, missed a section earlier");
        submission.GradedByTeacherId.Should().Be(secondTeacherId);
    }

    [Fact]
    public void Grade_DoesNotPersistMaxMarksParameterOntoMaxMarksProperty()
    {
        // Flagging this — it looks like a real gap, not just a documented quirk.
        // Grade(marksObtained, maxMarks, ...) validates against the *parameter* maxMarks,
        // but never assigns it to the entity's public MaxMarks property. So MaxMarks
        // stays at its default (0) no matter what's passed in here.
        var submission = CreateConfirmedSubmission();

        submission.Grade(80, 100, "Good work", TeacherId);

        submission.MaxMarks.Should().Be(0);
    }

    #endregion

    #region AddComment

    [Fact]
    public void AddComment_WhenStillDraft_ThrowsDomainException()
    {
        var submission = CreateDraftSubmission();

        var act = () => submission.AddComment("Nice effort", TeacherId);

        act.Should().Throw<DomainException>()
            .WithMessage("You can only grade or comment on a confirmed submission.");
    }

    [Fact]
    public void AddComment_WhenSubmitted_WithValidComment_TrimsAndSetsFeedback()
    {
        var submission = CreateConfirmedSubmission();
        var before = DateTime.UtcNow;

        submission.AddComment("  Nice effort  ", TeacherId);

        var after = DateTime.UtcNow;

        submission.Feedback.Should().Be("Nice effort");
        submission.GradedByTeacherId.Should().Be(TeacherId);
        submission.LastModifiedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void AddComment_DoesNotChangeGradeStatusOrGradedAt()
    {
        // AddComment sets GradedByTeacherId despite not actually grading anything —
        // this confirms it leaves GradeStatus/GradedAt untouched, so GradedByTeacherId
        // alone isn't a reliable signal that the submission has been graded.
        var submission = CreateConfirmedSubmission();

        submission.AddComment("Nice effort", TeacherId);

        submission.GradeStatus.Should().Be(GradeStatus.Pending);
        submission.GradedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddComment_WithInvalidComment_ThrowsDomainException(string? comment)
    {
        var submission = CreateConfirmedSubmission();

        var act = () => submission.AddComment(comment!, TeacherId);

        act.Should().Throw<DomainException>().WithMessage("Comment cannot be empty.");
    }

    #endregion

    #region IsLocked

    [Fact]
    public void IsLocked_WhileDraft_ReturnsFalse()
    {
        var submission = CreateDraftSubmission();

        submission.IsLocked().Should().BeFalse();
    }

    [Fact]
    public void IsLocked_AfterConfirm_ReturnsTrue()
    {
        var submission = CreateConfirmedSubmission();

        submission.IsLocked().Should().BeTrue();
    }

    #endregion

    #region Static extension helpers

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".PDF")]
    [InlineData(".docx")]
    [InlineData(".png")]
    public void IsAllowedExtension_WithKnownExtension_ReturnsTrue(string extension)
    {
        Submission.IsAllowedExtension(extension).Should().BeTrue();
    }

    [Theory]
    [InlineData(".exe")]
    [InlineData("pdf")] // missing leading dot — the set stores extensions with the dot included
    public void IsAllowedExtension_WithUnknownOrMalformedExtension_ReturnsFalse(string extension)
    {
        Submission.IsAllowedExtension(extension).Should().BeFalse();
    }

    [Fact]
    public void GetAllowedExtensions_ReturnsExpectedSet()
    {
        var extensions = Submission.GetAllowedExtensions();

        extensions.Should().BeEquivalentTo(new[]
        {
            ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        });
    }

    #endregion

    #region ReplaceFile

    [Fact]
    public void ReplaceFile_IsNotYetImplemented()
    {
        // ReplaceFile is currently a stub — `throw new NotImplementedException()` with no
        // logic at all. This test just pins down that fact so it fails loudly (as a build
        // break in CI, not a silent gap) once the method is actually implemented and this
        // test needs to be replaced with real coverage.
        var submission = CreateConfirmedSubmission();

        var act = () => submission.ReplaceFile("new.pdf", "blob-789", ValidContentType, ValidFileSizeBytes, isLate: false);

        act.Should().Throw<NotImplementedException>();
    }

    #endregion
}