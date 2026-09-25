using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ClassAssignmentSystem.Domain.UnitTests.Entities;

public class CourseTests
{
    private const string ValidTitle = "Introduction to Algorithms";
    private const string ValidDescription = "Covers sorting, searching, and graph traversal.";
    private const int ValidTotalSeats = 30;

    #region Create

    [Fact]
    public void Create_WithValidInputs_ReturnsActiveCourseWithTrimmedFields()
    {
        var before = DateTime.UtcNow;

        var course = Course.Create($"  {ValidTitle}  ", $"  {ValidDescription}  ", ValidTotalSeats);

        var after = DateTime.UtcNow;

        course.Id.Should().NotBe(Guid.Empty);
        course.Title.Should().Be(ValidTitle);
        course.Description.Should().Be(ValidDescription);
        course.TotalSeats.Should().Be(ValidTotalSeats);
        course.Status.Should().Be(CourseStatus.Active);
        course.TeacherId.Should().BeNull();
        course.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_CalledTwice_GeneratesUniqueIds()
    {
        var first = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);
        var second = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);

        first.Id.Should().NotBe(second.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ThrowsArgumentException(string? title)
    {
        var act = () => Course.Create(title!, ValidDescription, ValidTotalSeats);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ThrowsArgumentException(string? description)
    {
        var act = () => Course.Create(ValidTitle, description!, ValidTotalSeats);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("description");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithNonPositiveTotalSeats_ThrowsArgumentException(int totalSeats)
    {
        var act = () => Course.Create(ValidTitle, ValidDescription, totalSeats);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("totalSeats");
    }

    #endregion

    #region AssignTeacher / RemoveTeacher

    [Fact]
    public void AssignTeacher_SetsTeacherId()
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);
        var teacherId = Guid.NewGuid();

        course.AssignTeacher(teacherId);

        course.TeacherId.Should().Be(teacherId);
    }

    [Fact]
    public void AssignTeacher_CalledAgain_ReplacesPreviousTeacher()
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);
        course.AssignTeacher(Guid.NewGuid());
        var newTeacherId = Guid.NewGuid();

        course.AssignTeacher(newTeacherId);

        course.TeacherId.Should().Be(newTeacherId);
    }

    [Fact]
    public void RemoveTeacher_ClearsTeacherId()
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);
        course.AssignTeacher(Guid.NewGuid());

        course.RemoveTeacher();

        course.TeacherId.Should().BeNull();
    }

    [Fact]
    public void RemoveTeacher_WhenNoTeacherAssigned_LeavesTeacherIdNull()
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);

        course.RemoveTeacher();

        course.TeacherId.Should().BeNull();
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WithValidInputs_UpdatesAndTrimsFields()
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);

        course.Update("  Advanced Algorithms  ", "  Now with dynamic programming.  ", 45);

        course.Title.Should().Be("Advanced Algorithms");
        course.Description.Should().Be("Now with dynamic programming.");
        course.TotalSeats.Should().Be(45);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidTitle_ThrowsArgumentException(string? title)
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);

        var act = () => course.Update(title!, ValidDescription, ValidTotalSeats);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Update_WithNonPositiveTotalSeats_ThrowsArgumentException(int totalSeats)
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);

        var act = () => course.Update(ValidTitle, ValidDescription, totalSeats);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("totalSeats");
    }

    [Fact]
    public void Update_WithInvalidTitle_LeavesOriginalFieldsUnchanged()
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);

        var act = () => course.Update("", "New description", 50);
        act.Should().Throw<ArgumentException>();

        course.Title.Should().Be(ValidTitle);
        course.Description.Should().Be(ValidDescription);
        course.TotalSeats.Should().Be(ValidTotalSeats);
    }

    #endregion

    #region Activate / Deactivate

    [Fact]
    public void Deactivate_SetsStatusToInactive()
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);

        course.Deactivate();

        course.Status.Should().Be(CourseStatus.Inactive);
    }

    [Fact]
    public void Activate_AfterDeactivate_SetsStatusBackToActive()
    {
        var course = Course.Create(ValidTitle, ValidDescription, ValidTotalSeats);
        course.Deactivate();

        course.Activate();

        course.Status.Should().Be(CourseStatus.Active);
    }

    #endregion

    #region HasAvailableSeats

    [Fact]
    public void HasAvailableSeats_WhenApprovedCountBelowTotal_ReturnsTrue()
    {
        var course = Course.Create(ValidTitle, ValidDescription, 30);

        course.HasAvailableSeats(29).Should().BeTrue();
    }

    [Fact]
    public void HasAvailableSeats_WhenApprovedCountEqualsTotal_ReturnsFalse()
    {
        var course = Course.Create(ValidTitle, ValidDescription, 30);

        course.HasAvailableSeats(30).Should().BeFalse();
    }

    [Fact]
    public void HasAvailableSeats_WhenApprovedCountExceedsTotal_ReturnsFalse()
    {
        var course = Course.Create(ValidTitle, ValidDescription, 30);

        course.HasAvailableSeats(31).Should().BeFalse();
    }

    [Fact]
    public void HasAvailableSeats_WhenNoOneApproved_ReturnsTrue()
    {
        var course = Course.Create(ValidTitle, ValidDescription, 30);

        course.HasAvailableSeats(0).Should().BeTrue();
    }

    #endregion

    #region AvailableSeats

    [Fact]
    public void AvailableSeats_ReturnsRemainingSeatCount()
    {
        var course = Course.Create(ValidTitle, ValidDescription, 30);

        course.AvailableSeats(20).Should().Be(10);
    }

    [Fact]
    public void AvailableSeats_WhenApprovedCountEqualsTotal_ReturnsZero()
    {
        var course = Course.Create(ValidTitle, ValidDescription, 30);

        course.AvailableSeats(30).Should().Be(0);
    }

    [Fact]
    public void AvailableSeats_WhenApprovedCountExceedsTotal_ClampsToZero()
    {
        var course = Course.Create(ValidTitle, ValidDescription, 30);

        course.AvailableSeats(35).Should().Be(0);
    }

    [Fact]
    public void AvailableSeats_WhenNoOneApproved_ReturnsTotalSeats()
    {
        var course = Course.Create(ValidTitle, ValidDescription, 30);

        course.AvailableSeats(0).Should().Be(30);
    }

    #endregion
}