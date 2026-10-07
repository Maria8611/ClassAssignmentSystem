using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ClassAssignmentSystem.Domain.UnitTests.Entities;

public class UserTests
{
    private const string ValidFullName = "Jane Doe";
    private const string ValidEmail = "jane.doe@example.com";
    private const string ValidPasswordHash = "hashed-password-value";

    public static IEnumerable<object[]> FactoryMethods =>
        new List<object[]>
        {
            new object[] { (Func<string, string, string, User>)User.CreateTeacher, UserRole.Teacher },
            new object[] { (Func<string, string, string, User>)User.CreateStudent, UserRole.Student },
            new object[] { (Func<string, string, string, User>)User.CreateAdmin, UserRole.Admin },
        };

    #region Create (shared across CreateTeacher / CreateStudent / CreateAdmin)

    [Theory]
    [MemberData(nameof(FactoryMethods))]
    public void Create_WithValidInputs_SetsExpectedRoleAndIsActive(
        Func<string, string, string, User> factory, UserRole expectedRole)
    {
        var before = DateTime.UtcNow;

        var user = factory(ValidFullName, ValidEmail, ValidPasswordHash);

        var after = DateTime.UtcNow;

        user.Id.Should().NotBe(Guid.Empty);
        user.Role.Should().Be(expectedRole);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_TrimsFullName()
    {
        var user = User.CreateStudent($"  {ValidFullName}  ", ValidEmail, ValidPasswordHash);

        user.FullName.Should().Be(ValidFullName);
    }

    [Fact]
    public void Create_TrimsAndLowercasesEmail()
    {
        var user = User.CreateStudent(ValidFullName, "  Jane.Doe@EXAMPLE.com  ", ValidPasswordHash);

        user.Email.Should().Be("jane.doe@example.com");
    }

    [Fact]
    public void Create_DoesNotTrimPasswordHash()
    {
        const string hashWithWhitespace = "  hashed-password-value  ";

        var user = User.CreateStudent(ValidFullName, ValidEmail, hashWithWhitespace);

        user.PasswordHash.Should().Be(hashWithWhitespace);
    }

    [Fact]
    public void Create_CalledTwice_GeneratesUniqueIds()
    {
        var first = User.CreateStudent(ValidFullName, ValidEmail, ValidPasswordHash);
        var second = User.CreateStudent(ValidFullName, ValidEmail, ValidPasswordHash);

        first.Id.Should().NotBe(second.Id);
    }

    // Validation is implemented once in the shared private Create method, so it's
    // sufficient to exercise it through a single factory (CreateStudent) rather than
    // repeating these cases for CreateTeacher/CreateAdmin as well.

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFullName_ThrowsArgumentException(string? fullName)
    {
        var act = () => User.CreateStudent(fullName!, ValidEmail, ValidPasswordHash);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("fullName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ThrowsArgumentException(string? email)
    {
        var act = () => User.CreateStudent(ValidFullName, email!, ValidPasswordHash);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("email");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPasswordHash_ThrowsArgumentException(string? passwordHash)
    {
        var act = () => User.CreateStudent(ValidFullName, ValidEmail, passwordHash!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("passwordHash");
    }

    #endregion

    #region Activate / Deactivate

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var user = User.CreateStudent(ValidFullName, ValidEmail, ValidPasswordHash);

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_AfterDeactivate_SetsIsActiveBackToTrue()
    {
        var user = User.CreateStudent(ValidFullName, ValidEmail, ValidPasswordHash);
        user.Deactivate();

        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    #endregion

    #region UpdateName

    [Fact]
    public void UpdateName_WithValidName_TrimsAndUpdatesFullName()
    {
        var user = User.CreateStudent(ValidFullName, ValidEmail, ValidPasswordHash);

        user.UpdateName("  John Smith  ");

        user.FullName.Should().Be("John Smith");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_ThrowsArgumentException(string? fullName)
    {
        var user = User.CreateStudent(ValidFullName, ValidEmail, ValidPasswordHash);

        var act = () => user.UpdateName(fullName!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("fullName");
    }

    [Fact]
    public void UpdateName_WithInvalidName_LeavesOriginalNameUnchanged()
    {
        var user = User.CreateStudent(ValidFullName, ValidEmail, ValidPasswordHash);

        var act = () => user.UpdateName("   ");
        act.Should().Throw<ArgumentException>();

        user.FullName.Should().Be(ValidFullName);
    }

    #endregion
}