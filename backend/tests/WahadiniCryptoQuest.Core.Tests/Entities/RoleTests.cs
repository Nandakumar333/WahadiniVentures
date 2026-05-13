using FluentAssertions;
using WahadiniCryptoQuest.Core.Entities;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.Entities;

/// <summary>
/// Unit tests for Role, Permission, RolePermission, and UserRole entities
/// Tests RBAC entity creation, validation, and business logic
/// </summary>
public class RoleTests
{
    #region Role Entity Tests

    [Fact]
    public void Role_Create_WithValidData_ShouldSucceed()
    {
        // Act
        var role = Role.Create("Premium", "Premium subscription users");

        // Assert
        role.Should().NotBeNull();
        role.Id.Should().NotBeEmpty();
        role.Name.Should().Be("Premium");
        role.Description.Should().Be("Premium subscription users");
        role.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        role.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        role.UserRoles.Should().BeEmpty();
        role.RolePermissions.Should().BeEmpty();
    }

    [Fact]
    public void Role_Create_WithNullName_ShouldThrowArgumentException()
    {
        // Act
        var act = () => Role.Create(null!, "Description");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Role name is required*");
    }

    [Fact]
    public void Role_Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Act
        var act = () => Role.Create("  ", "Description");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Role name is required*");
    }

    [Fact]
    public void Role_Create_WithNameTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('A', 51);

        // Act
        var act = () => Role.Create(longName, "Description");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 50 characters*");
    }

    [Fact]
    public void Role_Create_WithNullDescription_ShouldUseEmptyString()
    {
        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var role = Role.Create("Free", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Assert
        role.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void Role_UpdateDescription_ShouldUpdateDescriptionAndTimestamp()
    {
        // Arrange
        var role = Role.Create("Admin", "Original description");
        var originalUpdatedAt = role.UpdatedAt;
        Thread.Sleep(10); // Ensure time difference

        // Act
        role.UpdateDescription("Updated description");

        // Assert
        role.Description.Should().Be("Updated description");
        role.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Role_AddPermission_ShouldAddPermissionToRole()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium users");
        var permission = Permission.Create("courses", "complete", "Complete courses");

        // Act
        role.AddPermission(permission);

        // Assert
        role.RolePermissions.Should().HaveCount(1);
        role.RolePermissions.First().PermissionId.Should().Be(permission.Id);
        role.RolePermissions.First().RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void Role_AddPermission_WithDuplicatePermission_ShouldNotAddTwice()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium users");
        var permission = Permission.Create("courses", "complete", "Complete courses");

        // Act
        role.AddPermission(permission);
        role.AddPermission(permission); // Try to add again

        // Assert
        role.RolePermissions.Should().HaveCount(1);
    }

    [Fact]
    public void Role_AddPermission_WithNullPermission_ShouldThrowArgumentNullException()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium users");

        // Act
        var act = () => role.AddPermission(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Role_RemovePermission_ShouldDeactivatePermission()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium users");
        var permission = Permission.Create("courses", "complete", "Complete courses");
        role.AddPermission(permission);

        // Act
        role.RemovePermission(permission.Id);

        // Assert
        var rolePermission = role.RolePermissions.First();
        rolePermission.IsActive.Should().BeFalse();
    }

    #endregion

    #region Permission Entity Tests

    [Fact]
    public void Permission_Create_WithValidData_ShouldSucceed()
    {
        // Act
        var permission = Permission.Create("courses", "create", "Create new courses");

        // Assert
        permission.Should().NotBeNull();
        permission.Id.Should().NotBeEmpty();
        permission.Name.Should().Be("courses:create");
        permission.Resource.Should().Be("courses");
        permission.Action.Should().Be("create");
        permission.Description.Should().Be("Create new courses");
        permission.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Permission_Create_WithUpperCaseInput_ShouldNormalizesToLowerCase()
    {
        // Act
        var permission = Permission.Create("COURSES", "CREATE", "Description");

        // Assert
        permission.Name.Should().Be("courses:create");
        permission.Resource.Should().Be("courses");
        permission.Action.Should().Be("create");
    }

    [Fact]
    public void Permission_Create_WithNullResource_ShouldThrowArgumentException()
    {
        // Act
        var act = () => Permission.Create(null!, "action");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Resource is required*");
    }

    [Fact]
    public void Permission_Create_WithNullAction_ShouldThrowArgumentException()
    {
        // Act
        var act = () => Permission.Create("resource", null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Action is required*");
    }

    [Fact]
    public void Permission_Create_WithResourceTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longResource = new string('A', 51);

        // Act
        var act = () => Permission.Create(longResource, "action");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Resource cannot exceed 50 characters*");
    }

    [Fact]
    public void Permission_Create_WithActionTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longAction = new string('A', 51);

        // Act
        var act = () => Permission.Create("resource", longAction);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Action cannot exceed 50 characters*");
    }

    [Fact]
    public void Permission_UpdateDescription_ShouldUpdateDescriptionAndTimestamp()
    {
        // Arrange
        var permission = Permission.Create("tasks", "review", "Original description");
        var originalUpdatedAt = permission.UpdatedAt;
        Thread.Sleep(10);

        // Act
        permission.UpdateDescription("Updated description");

        // Assert
        permission.Description.Should().Be("Updated description");
        permission.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    #endregion

    #region RolePermission Entity Tests

    [Fact]
    public void RolePermission_Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium users");
        var permission = Permission.Create("courses", "complete");

        // Act
        var rolePermission = RolePermission.Create(role, permission);

        // Assert
        rolePermission.Should().NotBeNull();
        rolePermission.Id.Should().NotBeEmpty();
        rolePermission.RoleId.Should().Be(role.Id);
        rolePermission.PermissionId.Should().Be(permission.Id);
        rolePermission.IsActive.Should().BeTrue();
        rolePermission.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RolePermission_Create_WithNullRole_ShouldThrowArgumentNullException()
    {
        // Arrange
        var permission = Permission.Create("courses", "complete");

        // Act
        var act = () => RolePermission.Create(null!, permission);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RolePermission_Create_WithNullPermission_ShouldThrowArgumentNullException()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium users");

        // Act
        var act = () => RolePermission.Create(role, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RolePermission_CreateFromIds_WithValidIds_ShouldSucceed()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        var rolePermission = RolePermission.CreateFromIds(roleId, permissionId);

        // Assert
        rolePermission.RoleId.Should().Be(roleId);
        rolePermission.PermissionId.Should().Be(permissionId);
        rolePermission.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RolePermission_CreateFromIds_WithEmptyRoleId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => RolePermission.CreateFromIds(Guid.Empty, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Role ID cannot be empty*");
    }

    [Fact]
    public void RolePermission_CreateFromIds_WithEmptyPermissionId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => RolePermission.CreateFromIds(Guid.NewGuid(), Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Permission ID cannot be empty*");
    }

    [Fact]
    public void RolePermission_Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium users");
        var permission = Permission.Create("courses", "complete");
        var rolePermission = RolePermission.Create(role, permission);

        // Act
        rolePermission.Deactivate();

        // Assert
        rolePermission.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RolePermission_Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium users");
        var permission = Permission.Create("courses", "complete");
        var rolePermission = RolePermission.Create(role, permission);
        rolePermission.Deactivate();

        // Act
        rolePermission.Activate();

        // Assert
        rolePermission.IsActive.Should().BeTrue();
    }

    #endregion

    #region UserRole Entity Tests

    [Fact]
    public void UserRole_Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var expiresAt = DateTime.UtcNow.AddMonths(1);

        // Act
        var userRole = UserRole.Create(user, role, expiresAt);

        // Assert
        userRole.Should().NotBeNull();
        userRole.Id.Should().NotBeEmpty();
        userRole.UserId.Should().Be(user.Id);
        userRole.RoleId.Should().Be(role.Id);
        userRole.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
        userRole.IsActive.Should().BeTrue();
        userRole.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UserRole_Create_WithoutExpiration_ShouldCreatePermanentRole()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Free", "Free users");

        // Act
        var userRole = UserRole.Create(user, role, null);

        // Assert
        userRole.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void UserRole_Create_WithPastExpiration_ShouldThrowArgumentException()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => UserRole.Create(user, role, pastDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Expiration date must be in the future*");
    }

    [Fact]
    public void UserRole_Create_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium users");

        // Act
        var act = () => UserRole.Create(null!, role);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UserRole_Create_WithNullRole_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");

        // Act
        var act = () => UserRole.Create(user, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UserRole_IsExpired_WithFutureExpiration_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = UserRole.Create(user, role, DateTime.UtcNow.AddDays(30));

        // Act
        var isExpired = userRole.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void UserRole_IsExpired_WithPastExpiration_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = UserRole.Create(user, role, DateTime.UtcNow.AddDays(1));
        
        // Use reflection to set past expiration (bypass validation)
        var expiresAtProperty = typeof(UserRole).GetProperty("ExpiresAt");
        expiresAtProperty!.SetValue(userRole, DateTime.UtcNow.AddDays(-1));

        // Act
        var isExpired = userRole.IsExpired();

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void UserRole_IsExpired_WithNullExpiration_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Free", "Free users");
        var userRole = UserRole.Create(user, role, null);

        // Act
        var isExpired = userRole.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void UserRole_Expire_ShouldSetExpirationToNowAndDeactivate()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = UserRole.Create(user, role, DateTime.UtcNow.AddMonths(1));

        // Act
        userRole.Expire();

        // Assert
        userRole.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        userRole.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UserRole_Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = UserRole.Create(user, role);

        // Act
        userRole.Deactivate();

        // Assert
        userRole.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UserRole_Activate_WhenNotExpired_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = UserRole.Create(user, role, DateTime.UtcNow.AddMonths(1));
        userRole.Deactivate();

        // Act
        userRole.Activate();

        // Assert
        userRole.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UserRole_Activate_WhenExpired_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = UserRole.Create(user, role, DateTime.UtcNow.AddDays(1));
        
        // Use reflection to set past expiration
        var expiresAtProperty = typeof(UserRole).GetProperty("ExpiresAt");
        expiresAtProperty!.SetValue(userRole, DateTime.UtcNow.AddDays(-1));
        userRole.Deactivate();

        // Act
        var act = () => userRole.Activate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot activate an expired role assignment*");
    }

    [Fact]
    public void UserRole_ExtendExpiration_WithFutureDate_ShouldUpdateExpiration()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = UserRole.Create(user, role, DateTime.UtcNow.AddMonths(1));
        var newExpiration = DateTime.UtcNow.AddMonths(3);

        // Act
        userRole.ExtendExpiration(newExpiration);

        // Assert
        userRole.ExpiresAt.Should().BeCloseTo(newExpiration, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UserRole_ExtendExpiration_WithPastDate_ShouldThrowArgumentException()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = UserRole.Create(user, role, DateTime.UtcNow.AddMonths(1));
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => userRole.ExtendExpiration(pastDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*New expiration date must be in the future*");
    }

    [Fact]
    public void UserRole_MakePermanent_ShouldRemoveExpiration()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = UserRole.Create(user, role, DateTime.UtcNow.AddMonths(1));

        // Act
        userRole.MakePermanent();

        // Assert
        userRole.ExpiresAt.Should().BeNull();
    }

    #endregion
}
