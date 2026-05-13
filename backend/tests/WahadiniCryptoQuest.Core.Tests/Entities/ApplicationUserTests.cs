using FluentAssertions;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.Entities;

/// <summary>
/// Tests for ApplicationUser entity
/// Coverage target: 100% line, 95%+ branch
/// </summary>
public class ApplicationUserTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.CreatedBy.Should().Be("System");
        user.UpdatedBy.Should().Be("System");
        user.IsDeleted.Should().BeFalse();
        user.Role.Should().Be(UserRoleEnum.Free);
        user.SecurityStamp.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Act
        var user1 = new ApplicationUser();
        var user2 = new ApplicationUser();

        // Assert
        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueSecurityStamps()
    {
        // Act
        var user1 = new ApplicationUser();
        var user2 = new ApplicationUser();

        // Assert
        user1.SecurityStamp.Should().NotBe(user2.SecurityStamp);
    }

    #endregion

    #region FullName Tests

    [Fact]
    public void FullName_ShouldCombineFirstAndLastName()
    {
        // Arrange
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void FullName_WithOnlyFirstName_ShouldReturnTrimmedFirstName()
    {
        // Arrange
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = ""
        };

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John");
    }

    [Fact]
    public void FullName_WithOnlyLastName_ShouldReturnTrimmedLastName()
    {
        // Arrange
        var user = new ApplicationUser
        {
            FirstName = "",
            LastName = "Doe"
        };

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("Doe");
    }

    [Fact]
    public void FullName_WithExtraWhitespace_ShouldTrimCorrectly()
    {
        // Arrange
        var user = new ApplicationUser
        {
            FirstName = "  John  ",
            LastName = "  Doe  "
        };

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John     Doe");
    }

    #endregion

    #region CreateFromDomainUser Tests

    [Fact]
    public void CreateFromDomainUser_WithValidUser_ShouldCreateApplicationUser()
    {
        // Arrange
        var domainUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        domainUser.ConfirmEmail();

        // Act
        var appUser = ApplicationUser.CreateFromDomainUser(domainUser);

        // Assert
        appUser.Should().NotBeNull();
        appUser.Id.Should().NotBe(Guid.Empty);
        appUser.DomainUserId.Should().Be(domainUser.Id);
        appUser.UserName.Should().Be("test@example.com");
        appUser.Email.Should().Be("test@example.com");
        appUser.NormalizedUserName.Should().Be("TEST@EXAMPLE.COM");
        appUser.NormalizedEmail.Should().Be("TEST@EXAMPLE.COM");
        appUser.FirstName.Should().Be("John");
        appUser.LastName.Should().Be("Doe");
        appUser.Role.Should().Be(UserRoleEnum.Free);
        appUser.EmailConfirmed.Should().BeTrue();
        appUser.LockoutEnabled.Should().BeTrue();
        appUser.SecurityStamp.Should().NotBeNullOrEmpty();
        appUser.DomainUser.Should().Be(domainUser);
    }

    [Fact]
    public void CreateFromDomainUser_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => ApplicationUser.CreateFromDomainUser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("domainUser");
    }

    [Fact]
    public void CreateFromDomainUser_WithDifferentRoles_ShouldPreserveRole()
    {
        // Arrange
        var domainUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        domainUser.UpgradeRole(UserRoleEnum.Premium);

        // Act
        var appUser = ApplicationUser.CreateFromDomainUser(domainUser);

        // Assert
        appUser.Role.Should().Be(UserRoleEnum.Premium);
    }

    [Fact]
    public void CreateFromDomainUser_ShouldSetAuditFields()
    {
        // Arrange
        var domainUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");

        // Act
        var appUser = ApplicationUser.CreateFromDomainUser(domainUser);

        // Assert
        appUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appUser.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appUser.CreatedBy.Should().Be("System");
        appUser.UpdatedBy.Should().Be("System");
    }

    #endregion

    #region SyncWithDomainUser Tests

    [Fact]
    public void SyncWithDomainUser_WithValidUser_ShouldUpdateAllFields()
    {
        // Arrange
        var domainUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        var appUser = ApplicationUser.CreateFromDomainUser(domainUser);

        // Modify domain user
        domainUser.UpdateProfile("Jane", "Smith");
        domainUser.UpgradeRole(UserRoleEnum.Premium);
        domainUser.ConfirmEmail();

        // Act
        appUser.SyncWithDomainUser(domainUser);

        // Assert
        appUser.FirstName.Should().Be("Jane");
        appUser.LastName.Should().Be("Smith");
        appUser.Role.Should().Be(UserRoleEnum.Premium);
        appUser.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void SyncWithDomainUser_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var appUser = new ApplicationUser();

        // Act
        var act = () => appUser.SyncWithDomainUser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("domainUser");
    }

    [Fact]
    public void SyncWithDomainUser_WithDifferentUserId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var domainUser1 = User.Create("test1@example.com", "hashedPassword", "John", "Doe");
        var domainUser2 = User.Create("test2@example.com", "hashedPassword", "Jane", "Smith");
        var appUser = ApplicationUser.CreateFromDomainUser(domainUser1);

        // Act
        var act = () => appUser.SyncWithDomainUser(domainUser2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot sync with different user entity");
    }

    [Fact]
    public void SyncWithDomainUser_WithLockedOutUser_ShouldSetLockoutEnd()
    {
        // Arrange
        var domainUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        var appUser = ApplicationUser.CreateFromDomainUser(domainUser);

        // Lock out the domain user
        domainUser.IncrementFailedLoginAttempts();
        domainUser.IncrementFailedLoginAttempts();
        domainUser.IncrementFailedLoginAttempts();
        domainUser.IncrementFailedLoginAttempts();
        domainUser.IncrementFailedLoginAttempts(); // 5 failed attempts
        domainUser.LockAccount(TimeSpan.FromDays(1)); // Manually lock

        // Act
        appUser.SyncWithDomainUser(domainUser);

        // Assert
        appUser.LockoutEnd.Should().Be(DateTimeOffset.MaxValue);
    }

    [Fact]
    public void SyncWithDomainUser_WithUnlockedUser_ShouldClearLockoutEnd()
    {
        // Arrange
        var domainUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        var appUser = ApplicationUser.CreateFromDomainUser(domainUser);
        appUser.LockoutEnd = DateTimeOffset.MaxValue;

        // User is not locked out anymore
        // Act
        appUser.SyncWithDomainUser(domainUser);

        // Assert
        appUser.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task SyncWithDomainUser_ShouldUpdateAuditFields()
    {
        // Arrange
        var domainUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        var appUser = ApplicationUser.CreateFromDomainUser(domainUser);
        var oldUpdateTime = appUser.UpdatedAt;

        await Task.Delay(10); // Small delay to ensure time difference

        // Act
        appUser.SyncWithDomainUser(domainUser);

        // Assert
        appUser.UpdatedAt.Should().BeAfter(oldUpdateTime);
        appUser.UpdatedBy.Should().Be("System");
    }

    [Fact]
    public void SyncWithDomainUser_WithNoDomainUserId_ShouldSetDomainUserId()
    {
        // Arrange
        var domainUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        var appUser = new ApplicationUser();

        // Act
        appUser.SyncWithDomainUser(domainUser);

        // Assert
        appUser.DomainUserId.Should().Be(domainUser.Id);
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.SoftDelete();

        // Assert
        user.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_ShouldUpdateTimestamp()
    {
        // Arrange
        var user = new ApplicationUser();
        var oldUpdateTime = user.UpdatedAt;

        // Act
        user.SoftDelete();

        // Assert
        user.UpdatedAt.Should().BeAfter(oldUpdateTime);
    }

    [Fact]
    public void SoftDelete_CalledMultipleTimes_ShouldRemainDeleted()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.SoftDelete();
        user.SoftDelete();

        // Assert
        user.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region UpdateAuditFields Tests

    [Fact]
    public void UpdateAuditFields_ShouldUpdateTimestamp()
    {
        // Arrange
        var user = new ApplicationUser();
        var oldUpdateTime = user.UpdatedAt;

        // Act
        user.UpdateAuditFields();

        // Assert
        user.UpdatedAt.Should().BeAfter(oldUpdateTime);
    }

    [Fact]
    public void UpdateAuditFields_WithDefaultParameter_ShouldSetSystemAsUpdater()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.UpdateAuditFields();

        // Assert
        user.UpdatedBy.Should().Be("System");
    }

    [Fact]
    public void UpdateAuditFields_WithCustomUpdater_ShouldSetCustomUpdater()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.UpdateAuditFields("admin@example.com");

        // Assert
        user.UpdatedBy.Should().Be("admin@example.com");
    }

    #endregion

    #region UpdateRole Tests

    [Fact]
    public void UpdateRole_ShouldChangeRole()
    {
        // Arrange
        var user = new ApplicationUser();
        user.Role.Should().Be(UserRoleEnum.Free); // Default

        // Act
        user.UpdateRole(UserRoleEnum.Premium);

        // Assert
        user.Role.Should().Be(UserRoleEnum.Premium);
    }

    [Fact]
    public void UpdateRole_ShouldUpdateAuditFields()
    {
        // Arrange
        var user = new ApplicationUser();
        var oldUpdateTime = user.UpdatedAt;

        // Act
        user.UpdateRole(UserRoleEnum.Premium);

        // Assert
        user.UpdatedAt.Should().BeAfter(oldUpdateTime);
        user.UpdatedBy.Should().Be("System");
    }

    [Fact]
    public void UpdateRole_WithCustomUpdater_ShouldSetCustomUpdater()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.UpdateRole(UserRoleEnum.Premium, "admin@example.com");

        // Assert
        user.Role.Should().Be(UserRoleEnum.Premium);
        user.UpdatedBy.Should().Be("admin@example.com");
    }

    [Fact]
    public void UpdateRole_WithAllRoleEnumValues_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new ApplicationUser();
        var roles = Enum.GetValues<UserRoleEnum>();

        // Act & Assert
        foreach (var role in roles)
        {
            user.UpdateRole(role);
            user.Role.Should().Be(role);
        }
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ApplicationUser_FullWorkflow_ShouldMaintainConsistency()
    {
        // Arrange
        var domainUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");

        // Act - Create from domain user
        var appUser = ApplicationUser.CreateFromDomainUser(domainUser);

        // Act - Update domain user (architecture: domain is source of truth)
        domainUser.UpdateProfile("Jane", "Smith");
        domainUser.UpgradeRole(UserRoleEnum.Premium);

        // Act - Sync ApplicationUser with updated domain user
        appUser.SyncWithDomainUser(domainUser);

        // Act - Soft delete on ApplicationUser
        appUser.SoftDelete();

        // Assert
        appUser.Role.Should().Be(UserRoleEnum.Premium);
        appUser.FirstName.Should().Be("Jane");
        appUser.LastName.Should().Be("Smith");
        appUser.IsDeleted.Should().BeTrue();
    }

    #endregion
}
