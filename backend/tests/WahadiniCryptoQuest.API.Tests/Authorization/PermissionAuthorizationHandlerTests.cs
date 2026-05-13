using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WahadiniCryptoQuest.API.Authorization;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
    {
        _handler = new PermissionAuthorizationHandler();
    }

    private ClaimsPrincipal CreateUserWithPermissions(params string[] permissions)
    {
        var claims = permissions.Select(p => new Claim("permission", p)).ToList();
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasRequiredPermission_Succeeds()
    {
        // Arrange
        var user = CreateUserWithPermissions("users.read", "users.write");
        var requirement = new PermissionAuthorizationRequirement("users.read");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserLacksRequiredPermission_Fails()
    {
        // Arrange
        var user = CreateUserWithPermissions("users.read");
        var requirement = new PermissionAuthorizationRequirement("users.delete");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeFalse(); // Not explicitly failed, just not succeeded
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasOneOfMultipleRequiredPermissions_Succeeds()
    {
        // Arrange (OR logic - any permission matches)
        var user = CreateUserWithPermissions("users.write");
        var requirement = new PermissionAuthorizationRequirement("users.read", "users.write", "users.delete");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasNoPermissions_Fails()
    {
        // Arrange
        var user = CreateUserWithPermissions(); // No permissions
        var requirement = new PermissionAuthorizationRequirement("users.read");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NotAuthenticatedUser_Fails()
    {
        // Arrange
        var user = new ClaimsPrincipal(); // No identity
        var requirement = new PermissionAuthorizationRequirement("users.read");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_CaseInsensitivePermissionMatch_Succeeds()
    {
        // Arrange
        var user = CreateUserWithPermissions("USERS.READ");
        var requirement = new PermissionAuthorizationRequirement("users.read");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasAllRequiredPermissions_Succeeds()
    {
        // Arrange
        var user = CreateUserWithPermissions("users.read", "users.write", "users.delete");
        var requirement = new PermissionAuthorizationRequirement("users.read", "users.write");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasSimilarButNotExactPermission_Fails()
    {
        // Arrange
        var user = CreateUserWithPermissions("users.readonly");
        var requirement = new PermissionAuthorizationRequirement("users.read");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_MultiplePermissionClaims_ChecksAll()
    {
        // Arrange
        var user = CreateUserWithPermissions("users.read", "content.read", "admin.full");
        var requirement = new PermissionAuthorizationRequirement("admin.full");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public void PermissionAuthorizationRequirement_WithNoPermissions_ThrowsArgumentException()
    {
        // Act
        Action act = () => new PermissionAuthorizationRequirement();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one permission must be specified*");
    }

    [Fact]
    public void PermissionAuthorizationRequirement_WithNullPermissions_ThrowsArgumentException()
    {
        // Act
        Action act = () => new PermissionAuthorizationRequirement(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one permission must be specified*");
    }

    [Fact]
    public void PermissionAuthorizationRequirement_WithValidPermissions_StoresPermissions()
    {
        // Arrange & Act
        var requirement = new PermissionAuthorizationRequirement("users.read", "users.write");

        // Assert
        requirement.Permissions.Should().HaveCount(2);
        requirement.Permissions.Should().Contain("users.read");
        requirement.Permissions.Should().Contain("users.write");
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithDuplicatePermissions_HandlesCorrectly()
    {
        // Arrange - User has duplicate permission claims
        var claims = new List<Claim>
        {
            new Claim("permission", "users.read"),
            new Claim("permission", "users.read"),
            new Claim("permission", "users.write")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        
        var requirement = new PermissionAuthorizationRequirement("users.read");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithOtherClaimTypes_IgnoresNonPermissionClaims()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("permission", "users.read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        
        var requirement = new PermissionAuthorizationRequirement("users.read");
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
}
