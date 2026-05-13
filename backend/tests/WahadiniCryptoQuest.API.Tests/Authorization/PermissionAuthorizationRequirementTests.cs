using FluentAssertions;
using WahadiniCryptoQuest.API.Authorization;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Authorization;

/// <summary>
/// Tests for PermissionAuthorizationRequirement
/// Coverage target: 100% line, 95%+ branch
/// </summary>
public class PermissionAuthorizationRequirementTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidSinglePermission_ShouldSetPermissions()
    {
        // Arrange & Act
        var requirement = new PermissionAuthorizationRequirement("courses:view");

        // Assert
        requirement.Permissions.Should().NotBeNull();
        requirement.Permissions.Should().HaveCount(1);
        requirement.Permissions[0].Should().Be("courses:view");
    }

    [Fact]
    public void Constructor_WithValidMultiplePermissions_ShouldSetAllPermissions()
    {
        // Arrange & Act
        var requirement = new PermissionAuthorizationRequirement("courses:view", "courses:edit", "courses:delete");

        // Assert
        requirement.Permissions.Should().NotBeNull();
        requirement.Permissions.Should().HaveCount(3);
        requirement.Permissions.Should().Contain(new[] { "courses:view", "courses:edit", "courses:delete" });
    }

    [Fact]
    public void Constructor_WithNullPermissions_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = () => new PermissionAuthorizationRequirement(null!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one permission must be specified*")
            .WithParameterName("permissions");
    }

    [Fact]
    public void Constructor_WithEmptyPermissionsArray_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = () => new PermissionAuthorizationRequirement(Array.Empty<string>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one permission must be specified*")
            .WithParameterName("permissions");
    }

    #endregion

    #region Permissions Property Tests

    [Fact]
    public void Permissions_ShouldReturnAllProvidedPermissions()
    {
        // Arrange
        var expectedPermissions = new[] { "courses:view", "lessons:edit", "tasks:approve" };

        // Act
        var requirement = new PermissionAuthorizationRequirement(expectedPermissions);

        // Assert
        requirement.Permissions.Should().BeEquivalentTo(expectedPermissions);
    }

    [Fact]
    public void Permissions_WithDuplicates_ShouldStoreDuplicates()
    {
        // Arrange & Act
        var requirement = new PermissionAuthorizationRequirement("courses:view", "courses:view", "courses:view");

        // Assert
        requirement.Permissions.Should().HaveCount(3);
        requirement.Permissions.Should().AllBe("courses:view");
    }

    [Fact]
    public void Permissions_ShouldPreserveOrderOfPermissions()
    {
        // Arrange
        var orderedPermissions = new[] { "zebra:action", "alpha:action", "mike:action" };

        // Act
        var requirement = new PermissionAuthorizationRequirement(orderedPermissions);

        // Assert
        requirement.Permissions.Should().ContainInOrder(orderedPermissions);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Constructor_WithManyPermissions_ShouldStoreAll()
    {
        // Arrange
        var manyPermissions = new[]
        {
            "courses:view", "courses:edit", "courses:delete",
            "lessons:view", "lessons:edit", "lessons:delete",
            "tasks:view", "tasks:approve", "tasks:reject",
            "users:view", "users:manage", "admin:full"
        };

        // Act
        var requirement = new PermissionAuthorizationRequirement(manyPermissions);

        // Assert
        requirement.Permissions.Should().HaveCount(12);
        requirement.Permissions.Should().Contain(manyPermissions);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInPermissions_ShouldStoreUnmodified()
    {
        // Arrange & Act
        var requirement = new PermissionAuthorizationRequirement(
            "crypto-courses:view-premium",
            "user_profiles:edit_own",
            "admin.dashboard:view.analytics"
        );

        // Assert
        requirement.Permissions.Should().Contain("crypto-courses:view-premium");
        requirement.Permissions.Should().Contain("user_profiles:edit_own");
        requirement.Permissions.Should().Contain("admin.dashboard:view.analytics");
    }

    [Fact]
    public void Constructor_WithEmptyStringPermission_ShouldStoreEmptyString()
    {
        // Arrange & Act - No validation in the requirement itself
        var requirement = new PermissionAuthorizationRequirement("", "courses:view");

        // Assert
        requirement.Permissions.Should().Contain("");
        requirement.Permissions.Should().Contain("courses:view");
    }

    [Fact]
    public void Constructor_WithWhitespacePermission_ShouldStoreWhitespace()
    {
        // Arrange & Act - No validation in the requirement itself
        var requirement = new PermissionAuthorizationRequirement("   ", "courses:view");

        // Assert
        requirement.Permissions.Should().Contain("   ");
        requirement.Permissions.Should().Contain("courses:view");
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Requirement_ShouldImplementIAuthorizationRequirement()
    {
        // Arrange & Act
        var requirement = new PermissionAuthorizationRequirement("courses:view");

        // Assert
        requirement.Should().BeAssignableTo<Microsoft.AspNetCore.Authorization.IAuthorizationRequirement>();
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void Requirement_ForSinglePermissionCheck_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var requirement = new PermissionAuthorizationRequirement("courses:view");

        // Assert
        requirement.Permissions.Should().HaveCount(1);
        requirement.Permissions[0].Should().Be("courses:view");
    }

    [Fact]
    public void Requirement_ForORLogicWithMultiplePermissions_ShouldStoreAll()
    {
        // Arrange & Act - User needs ANY of these permissions (OR logic)
        var requirement = new PermissionAuthorizationRequirement(
            "courses:view",
            "courses:view-premium",
            "admin:full"
        );

        // Assert
        requirement.Permissions.Should().HaveCount(3);
        requirement.Permissions.Should().Contain(new[] { "courses:view", "courses:view-premium", "admin:full" });
    }

    [Fact]
    public void Requirement_WithComplexPermissionHierarchy_ShouldStoreFlat()
    {
        // Arrange & Act
        var requirement = new PermissionAuthorizationRequirement(
            "admin:full",
            "courses:manage",
            "courses:edit",
            "courses:view"
        );

        // Assert - All stored at same level, no hierarchy
        requirement.Permissions.Should().HaveCount(4);
        requirement.Permissions.Should().Contain("admin:full");
        requirement.Permissions.Should().Contain("courses:manage");
        requirement.Permissions.Should().Contain("courses:edit");
        requirement.Permissions.Should().Contain("courses:view");
    }

    #endregion
}
