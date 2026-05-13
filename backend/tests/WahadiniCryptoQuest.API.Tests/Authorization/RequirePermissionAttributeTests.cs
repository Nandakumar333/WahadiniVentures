using FluentAssertions;
using WahadiniCryptoQuest.API.Authorization;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Authorization;

/// <summary>
/// Tests for RequirePermissionAttribute custom authorization attribute
/// Coverage target: 100% line, 95%+ branch
/// </summary>
public class RequirePermissionAttributeTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidSinglePermission_ShouldSetPermissionsAndPolicy()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute("courses:view");

        // Assert
        attribute.Permissions.Should().NotBeNull();
        attribute.Permissions.Should().HaveCount(1);
        attribute.Permissions[0].Should().Be("courses:view");
        attribute.Policy.Should().Be("RequirePermission");
    }

    [Fact]
    public void Constructor_WithValidMultiplePermissions_ShouldSetAllPermissions()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute("courses:view", "courses:edit", "courses:delete");

        // Assert
        attribute.Permissions.Should().NotBeNull();
        attribute.Permissions.Should().HaveCount(3);
        attribute.Permissions.Should().Contain(new[] { "courses:view", "courses:edit", "courses:delete" });
        attribute.Policy.Should().Be("RequirePermission");
    }

    [Fact]
    public void Constructor_WithNullPermissions_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = () => new RequirePermissionAttribute(null!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one permission must be specified*")
            .WithParameterName("permissions");
    }

    [Fact]
    public void Constructor_WithEmptyPermissionsArray_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = () => new RequirePermissionAttribute(Array.Empty<string>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one permission must be specified*")
            .WithParameterName("permissions");
    }

    #endregion

    #region Permission Format Validation Tests

    [Theory]
    [InlineData("courses:view")]
    [InlineData("lessons:edit")]
    [InlineData("users:delete")]
    [InlineData("tasks:approve")]
    [InlineData("admin:manage")]
    public void Constructor_WithValidPermissionFormat_ShouldSucceed(string permission)
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute(permission);

        // Assert
        attribute.Permissions.Should().Contain(permission);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespacePermission_ShouldThrowArgumentException(string invalidPermission)
    {
        // Arrange, Act & Assert
        var act = () => new RequirePermissionAttribute(invalidPermission);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Permission*must follow 'resource:action' format*")
            .WithParameterName("permissions");
    }

    [Theory]
    [InlineData("courses")]
    [InlineData("coursesview")]
    [InlineData("courses_view")]
    [InlineData("courses-view")]
    [InlineData("courses.view")]
    public void Constructor_WithInvalidPermissionFormat_ShouldThrowArgumentException(string invalidPermission)
    {
        // Arrange, Act & Assert
        var act = () => new RequirePermissionAttribute(invalidPermission);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"Permission '{invalidPermission}' must follow 'resource:action' format*")
            .WithParameterName("permissions");
    }

    [Fact]
    public void Constructor_WithMixOfValidAndInvalidPermissions_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = () => new RequirePermissionAttribute("courses:view", "invalid-format", "lessons:edit");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Permission 'invalid-format' must follow 'resource:action' format*")
            .WithParameterName("permissions");
    }

    [Fact]
    public void Constructor_WithNullPermissionInArray_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = () => new RequirePermissionAttribute("courses:view", null!, "lessons:edit");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Permission*must follow 'resource:action' format*")
            .WithParameterName("permissions");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Constructor_WithComplexPermissionNames_ShouldSucceed()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute(
            "crypto-courses:view-premium",
            "user-profiles:edit-own",
            "admin-dashboard:view-analytics"
        );

        // Assert
        attribute.Permissions.Should().HaveCount(3);
        attribute.Permissions.Should().Contain("crypto-courses:view-premium");
        attribute.Permissions.Should().Contain("user-profiles:edit-own");
        attribute.Permissions.Should().Contain("admin-dashboard:view-analytics");
    }

    [Fact]
    public void Constructor_WithMultipleColons_ShouldSucceed()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute("resource:sub-resource:action");

        // Assert
        attribute.Permissions.Should().Contain("resource:sub-resource:action");
    }

    [Fact]
    public void Constructor_WithDuplicatePermissions_ShouldStoreDuplicates()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute("courses:view", "courses:view", "courses:view");

        // Assert
        attribute.Permissions.Should().HaveCount(3);
        attribute.Permissions.Should().AllBe("courses:view");
    }

    [Fact]
    public void Permissions_ShouldBeReadOnly_ButArrayContentsAreMutable()
    {
        // Arrange
        var attribute = new RequirePermissionAttribute("courses:view", "courses:edit");

        // Act - Try to modify the array contents (allowed by .NET arrays)
        var permissions = attribute.Permissions;
        permissions[0] = "modified:permission";

        // Assert - The property returns the same reference, so mutation is visible
        attribute.Permissions[0].Should().Be("modified:permission");
        // Note: This is a .NET limitation - arrays are always mutable
        // A truly immutable design would use IReadOnlyList<string>
    }

    [Fact]
    public void Policy_ShouldAlwaysBeRequirePermission()
    {
        // Arrange & Act
        var attribute1 = new RequirePermissionAttribute("courses:view");
        var attribute2 = new RequirePermissionAttribute("users:manage", "admin:full");
        var attribute3 = new RequirePermissionAttribute("tasks:approve");

        // Assert
        attribute1.Policy.Should().Be("RequirePermission");
        attribute2.Policy.Should().Be("RequirePermission");
        attribute3.Policy.Should().Be("RequirePermission");
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void Attribute_CanBeUsedWithSinglePermission_ForBasicAuthorization()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute("courses:view");

        // Assert
        attribute.Permissions.Should().HaveCount(1);
        attribute.Permissions[0].Should().Be("courses:view");
        attribute.Policy.Should().Be("RequirePermission");
    }

    [Fact]
    public void Attribute_CanBeUsedWithMultiplePermissions_ForORLogic()
    {
        // Arrange & Act - User needs ANY of these permissions (OR logic)
        var attribute = new RequirePermissionAttribute("courses:view", "courses:view-premium", "admin:full");

        // Assert
        attribute.Permissions.Should().HaveCount(3);
        attribute.Permissions.Should().Contain(new[] { "courses:view", "courses:view-premium", "admin:full" });
    }

    [Fact]
    public void Attribute_WithManyPermissions_ShouldStoreAll()
    {
        // Arrange - Simulating a complex authorization scenario
        var manyPermissions = new[]
        {
            "courses:view", "courses:edit", "courses:delete",
            "lessons:view", "lessons:edit", "lessons:delete",
            "tasks:view", "tasks:approve", "tasks:reject",
            "users:view", "users:manage", "admin:full"
        };

        // Act
        var attribute = new RequirePermissionAttribute(manyPermissions);

        // Assert
        attribute.Permissions.Should().HaveCount(12);
        attribute.Permissions.Should().Contain(manyPermissions);
    }

    #endregion
}
