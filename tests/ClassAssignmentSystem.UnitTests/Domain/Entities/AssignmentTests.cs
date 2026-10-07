using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using FluentAssertions;
using System.Threading;
using Xunit;

namespace ClassAssignmentSystem.Domain.UnitTests.Entities;

public class AssignmentTests
{
    private const string ValidTitle = "Homework 1";
    private const string ValidDescription = "Solve the attached problem set.";
    private const int ValidMaxMarks = 100;
    private static readonly Guid TeacherId = Guid.NewGuid();
    private static readonly Guid CourseId = Guid.NewGuid();

    private static Assignment CreateValidAssignment(
        Guid? teacherId = null, DateTime? deadline = null, int maxMarks = ValidMaxMarks)
        => Assignment.Create(
            ValidTitle,
            ValidDescription,
            deadline ?? DateTime.UtcNow.AddDays(7),
            maxMarks,
            CourseId,
            teacherId ?? TeacherId);

    #region Create

    [Fact]
    public void Create_WithValidInputs_SetsExpectedFieldsAndTrims()
    {
        var deadline = DateTime.UtcNow.AddDays(7);
        var before = DateTime.UtcNow;

        var assignment = Assignment.Create(
            $"  {ValidTitle}  ", $"  {ValidDescription}  ", deadline, ValidMaxMarks, CourseId, TeacherId);

        var after = DateTime.UtcNow;

        assignment.Id.Should().NotBe(Guid.Empty);
        assignment.Title.Should().Be(ValidTitle);
        assignment.Description.Should().Be(ValidDescription);
        assignment.Deadline.Should().Be(deadline);
        assignment.MaxMarks.Should().Be(ValidMaxMarks);
        assignment.CourseId.Should().Be(CourseId);
        assignment.CreatedByTeacherId.Should().Be(TeacherId);
        assignment.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_CalledTwice_GeneratesUniqueIds()
    {
        var first = CreateValidAssignment();
        var second = CreateValidAssignment();

        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Create_LeavesFileMetadataAtDefaultsUntilUploaded()
    {
        var assignment = CreateValidAssignment();

        assignment.FileName.Should().BeEmpty();
        assignment.StoredFileName.Should().BeEmpty();
        assignment.ContentType.Should().BeEmpty();
        assignment.FileSizeBytes.Should().BeNull();
        assignment.LastModifiedAt.Should().Be(default);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ThrowsArgumentException(string? title)
    {
        var act = () => Assignment.Create(
            title!, ValidDescription, DateTime.UtcNow.AddDays(1), ValidMaxMarks, CourseId, TeacherId);

        act.Should().Throw<ArgumentException>().WithMessage("Title is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ThrowsArgumentException(string? description)
    {
        var act = () => Assignment.Create(
            ValidTitle, description!, DateTime.UtcNow.AddDays(1), ValidMaxMarks, CourseId, TeacherId);

        act.Should().Throw<ArgumentException>().WithMessage("Description is required.");
    }

    [Fact]
    public void Create_WithDeadlineInThePast_ThrowsDomainException()
    {
        var act = () => Assignment.Create(
            ValidTitle, ValidDescription, DateTime.UtcNow.AddDays(-1), ValidMaxMarks, CourseId, TeacherId);

        act.Should().Throw<DomainException>().WithMessage("Deadline must be in the future.");
    }

    [Fact]
    public void Create_WithDeadlineEqualToNow_ThrowsDomainException()
    {
        // By the time the entity's internal `deadline <= DateTime.UtcNow` check runs,
        // real time has advanced past this captured instant, so it's already "the past".
        var deadline = DateTime.UtcNow;

        var act = () => Assignment.Create(
            ValidTitle, ValidDescription, deadline, ValidMaxMarks, CourseId, TeacherId);

        act.Should().Throw<DomainException>().WithMessage("Deadline must be in the future.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithNonPositiveMaxMarks_ThrowsDomainException(int maxMarks)
    {
        var act = () => CreateValidAssignment(maxMarks: maxMarks);

        act.Should().Throw<DomainException>().WithMessage("Max marks must be greater than zero.");
    }

    #endregion

    #region Update

    [Fact]
    public void Update_ByAssignedTeacher_WithValidInputs_UpdatesFields()
    {
        var assignment = CreateValidAssignment();
        var newDeadline = DateTime.UtcNow.AddDays(14);

        assignment.Update("  Homework 2  ", "  Revised problem set.  ", newDeadline, 50, TeacherId);

        assignment.Title.Should().Be("Homework 2");
        assignment.Description.Should().Be("Revised problem set.");
        assignment.Deadline.Should().Be(newDeadline);
        assignment.MaxMarks.Should().Be(50);
    }

    [Fact]
    public void Update_ByTeacherWhoDoesNotOwnAssignment_ThrowsDomainException()
    {
        var assignment = CreateValidAssignment();
        var otherTeacherId = Guid.NewGuid();

        var act = () => assignment.Update(
            "New title", "New description", DateTime.UtcNow.AddDays(1), 50, otherTeacherId);

        act.Should().Throw<DomainException>()
            .WithMessage("You are not the teacher assigned to this course and cannot modify this assignment.");
    }

    [Fact]
    public void Update_OwnershipIsCheckedBeforeFieldValidation()
    {
        var assignment = CreateValidAssignment();
        var otherTeacherId = Guid.NewGuid();

        // Title is also invalid here, but ownership should fail first —
        // proves EnsureOwnership runs before the argument checks, not after.
        var act = () => assignment.Update("", "", DateTime.UtcNow.AddDays(-1), -1, otherTeacherId);

        act.Should().Throw<DomainException>()
            .WithMessage("You are not the teacher assigned to this course and cannot modify this assignment.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidTitle_ThrowsArgumentException(string? title)
    {
        var assignment = CreateValidAssignment();

        var act = () => assignment.Update(
            title!, ValidDescription, DateTime.UtcNow.AddDays(1), ValidMaxMarks, TeacherId);

        act.Should().Throw<ArgumentException>().WithMessage("Title is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidDescription_ThrowsArgumentException(string? description)
    {
        var assignment = CreateValidAssignment();

        var act = () => assignment.Update(
            ValidTitle, description!, DateTime.UtcNow.AddDays(1), ValidMaxMarks, TeacherId);

        act.Should().Throw<ArgumentException>().WithMessage("Description is required.");
    }

    [Fact]
    public void Update_WithDeadlineInThePast_ThrowsDomainException()
    {
        var assignment = CreateValidAssignment();

        var act = () => assignment.Update(
            ValidTitle, ValidDescription, DateTime.UtcNow.AddDays(-1), ValidMaxMarks, TeacherId);

        act.Should().Throw<DomainException>().WithMessage("Deadline must be in the future.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Update_WithNonPositiveMaxMarks_ThrowsDomainException(int maxMarks)
    {
        var assignment = CreateValidAssignment();

        var act = () => assignment.Update(
            ValidTitle, ValidDescription, DateTime.UtcNow.AddDays(1), maxMarks, TeacherId);

        act.Should().Throw<DomainException>().WithMessage("Max marks must be greater than zero.");
    }

    [Fact]
    public void Update_WhenValidationFails_LeavesOriginalFieldsUnchanged()
    {
        var originalDeadline = DateTime.UtcNow.AddDays(7);
        var assignment = CreateValidAssignment(deadline: originalDeadline);

        var act = () => assignment.Update("", "New description", DateTime.UtcNow.AddDays(14), 50, TeacherId);
        act.Should().Throw<ArgumentException>();

        assignment.Title.Should().Be(ValidTitle);
        assignment.Description.Should().Be(ValidDescription);
        assignment.Deadline.Should().Be(originalDeadline);
        assignment.MaxMarks.Should().Be(ValidMaxMarks);
    }

    #endregion

    #region IsDeadlinePassed

    [Fact]
    public void IsDeadlinePassed_BeforeDeadline_ReturnsFalse()
    {
        var assignment = CreateValidAssignment(deadline: DateTime.UtcNow.AddDays(1));

        assignment.IsDeadlinePassed().Should().BeFalse();
    }

    [Fact]
    public void IsDeadlinePassed_AfterDeadline_ReturnsTrue()
    {
        // The entity compares against DateTime.UtcNow directly with no injectable clock,
        // so this case can only be exercised by actually waiting for real time to pass.
        // If this test becomes flaky in CI, consider adding an IDateTimeProvider abstraction.
        var assignment = CreateValidAssignment(deadline: DateTime.UtcNow.AddMilliseconds(150));

        Thread.Sleep(300);

        assignment.IsDeadlinePassed().Should().BeTrue();
    }

    #endregion

    #region SetMaterialBlobName

    [Fact]
    public void SetMaterialBlobName_SetsStoredFileName()
    {
        var assignment = CreateValidAssignment();

        assignment.SetMaterialBlobName("new-blob-name.bin");

        assignment.StoredFileName.Should().Be("new-blob-name.bin");
    }

    #endregion

    #region UploadAssignmentMaterial

    [Fact]
    public void UploadAssignmentMaterial_WithAllowedFile_SetsMetadataAndTimestamp()
    {
        var assignment = CreateValidAssignment();
        var before = DateTime.UtcNow;

        assignment.UploadAssignmentMaterial("syllabus.pdf", "blob-123", "application/pdf", 204_800);

        var after = DateTime.UtcNow;

        assignment.FileName.Should().Be("syllabus.pdf");
        assignment.StoredFileName.Should().Be("blob-123");
        assignment.ContentType.Should().Be("application/pdf");
        assignment.FileSizeBytes.Should().Be(204_800);
        assignment.LastModifiedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
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
    public void UploadAssignmentMaterial_WithAllowedExtensionContentTypePairs_DoesNotThrow(
        string fileName, string contentType)
    {
        var assignment = CreateValidAssignment();

        var act = () => assignment.UploadAssignmentMaterial(fileName, "blob-name", contentType, 1024);

        act.Should().NotThrow();
    }

    [Fact]
    public void UploadAssignmentMaterial_ExtensionAndContentTypeMatchingIsCaseInsensitive()
    {
        var assignment = CreateValidAssignment();

        var act = () => assignment.UploadAssignmentMaterial("Photo.JPG", "blob-name", "IMAGE/JPEG", 2048);

        act.Should().NotThrow();
    }

    [Fact]
    public void UploadAssignmentMaterial_WithDisallowedExtension_ThrowsDomainException()
    {
        var assignment = CreateValidAssignment();

        var act = () => assignment.UploadAssignmentMaterial("installer.exe", "blob-name", "application/pdf", 1024);

        act.Should().Throw<DomainException>()
            .WithMessage("File type '.exe' is not allowed. Accepted: PDF, Word, PowerPoint, Excel, and common image formats.");
    }

    [Fact]
    public void UploadAssignmentMaterial_WithAllowedExtensionButDisallowedContentType_ThrowsDomainException()
    {
        var assignment = CreateValidAssignment();

        var act = () => assignment.UploadAssignmentMaterial("report.pdf", "blob-name", "application/octet-stream", 1024);

        act.Should().Throw<DomainException>()
            .WithMessage("Content type 'application/octet-stream' is not permitted.");
    }

    [Fact]
    public void UploadAssignmentMaterial_CalledAgain_OverwritesPreviousMetadata()
    {
        var assignment = CreateValidAssignment();
        assignment.UploadAssignmentMaterial("first.pdf", "blob-1", "application/pdf", 1000);

        assignment.UploadAssignmentMaterial("second.png", "blob-2", "image/png", 2000);

        assignment.FileName.Should().Be("second.png");
        assignment.StoredFileName.Should().Be("blob-2");
        assignment.ContentType.Should().Be("image/png");
        assignment.FileSizeBytes.Should().Be(2000);
    }

    [Fact]
    public void UploadAssignmentMaterial_WithNonPositiveFileSize_DoesNotThrow()
    {
        // Documents current behavior: fileSizeBytes has no validation at all, so
        // zero or negative sizes are accepted as-is. Flagging in case this was unintentional.
        var assignment = CreateValidAssignment();

        var act = () => assignment.UploadAssignmentMaterial("report.pdf", "blob-name", "application/pdf", -100);

        act.Should().NotThrow();
        assignment.FileSizeBytes.Should().Be(-100);
    }

    [Fact]
    public void UploadAssignmentMaterial_WithNullFileName_IsTreatedAsDisallowedExtension()
    {
        // Path.GetExtension(null) returns null rather than throwing, so a null fileName
        // doesn't crash — it just fails extension validation with an empty extension in the message.
        var assignment = CreateValidAssignment();

        var act = () => assignment.UploadAssignmentMaterial(null!, "blob-name", "application/pdf", 1024);

        act.Should().Throw<DomainException>()
            .WithMessage("File type '' is not allowed. Accepted: PDF, Word, PowerPoint, Excel, and common image formats.");
    }

    #endregion
}