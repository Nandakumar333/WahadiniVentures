using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WahadiniCryptoQuest.Core.Authorization;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Service.Handlers.Authorization;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Authorization;

/// <summary>
/// Unit tests for authorization handlers
/// </summary>
public class AuthorizationHandlersTests
{
    #region PermissionRequirementHandler Tests

    [Fact]
    public async Task PermissionRequirementHandler_WithValidPermissionClaim_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<PermissionRequirementHandler>>();
        var handler = new PermissionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "true"),
            new("permission", "courses:read"),
            new("permission", "courses:write")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement("courses:read");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task PermissionRequirementHandler_WithoutPermissionClaim_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<PermissionRequirementHandler>>();
        var handler = new PermissionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "true"),
            new("permission", "courses:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement("courses:delete");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task PermissionRequirementHandler_WithoutUserIdClaim_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<PermissionRequirementHandler>>();
        var handler = new PermissionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new("permission", "courses:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement("courses:read");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task PermissionRequirementHandler_WithUnconfirmedEmailWhenRequired_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<PermissionRequirementHandler>>();
        var handler = new PermissionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "false"),
            new("permission", "courses:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement("courses:read", requireEmailConfirmation: true);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task PermissionRequirementHandler_WithUnconfirmedEmailWhenNotRequired_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<PermissionRequirementHandler>>();
        var handler = new PermissionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "false"),
            new("permission", "courses:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement("courses:read", requireEmailConfirmation: false);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region SubscriptionRequirementHandler Tests

    [Fact]
    public async Task SubscriptionRequirementHandler_WithSufficientRole_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionRequirementHandler>>();
        var handler = new SubscriptionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "true"),
            new(ClaimTypes.Role, "Premium")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new SubscriptionRequirement(UserRoleEnum.Free);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task SubscriptionRequirementHandler_WithInsufficientRole_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionRequirementHandler>>();
        var handler = new SubscriptionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "true"),
            new(ClaimTypes.Role, "Free")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new SubscriptionRequirement(UserRoleEnum.Premium);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task SubscriptionRequirementHandler_AdminRoleForPremiumRequirement_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionRequirementHandler>>();
        var handler = new SubscriptionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "true"),
            new(ClaimTypes.Role, "Admin")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new SubscriptionRequirement(UserRoleEnum.Premium);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task SubscriptionRequirementHandler_WithoutRoleClaim_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionRequirementHandler>>();
        var handler = new SubscriptionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "true")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new SubscriptionRequirement(UserRoleEnum.Free);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task SubscriptionRequirementHandler_WithInvalidRoleValue_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionRequirementHandler>>();
        var handler = new SubscriptionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "true"),
            new(ClaimTypes.Role, "InvalidRole")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new SubscriptionRequirement(UserRoleEnum.Free);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task SubscriptionRequirementHandler_WithUnconfirmedEmailWhenRequired_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionRequirementHandler>>();
        var handler = new SubscriptionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "false"),
            new(ClaimTypes.Role, "Premium")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new SubscriptionRequirement(UserRoleEnum.Premium, requireEmailConfirmation: true);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task SubscriptionRequirementHandler_WithUnconfirmedEmailWhenNotRequired_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionRequirementHandler>>();
        var handler = new SubscriptionRequirementHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("email_verified", "false"),
            new(ClaimTypes.Role, "Premium")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new SubscriptionRequirement(UserRoleEnum.Premium, requireEmailConfirmation: false);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region RoleHandler Tests

    [Fact]
    public async Task RoleHandler_WithMatchingRole_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<RoleHandler>>();
        var handler = new RoleHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new(ClaimTypes.Role, "Admin")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new RolesAuthorizationRequirement(new[] { "Admin", "Manager" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task RoleHandler_WithoutMatchingRole_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<RoleHandler>>();
        var handler = new RoleHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new(ClaimTypes.Role, "User")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new RolesAuthorizationRequirement(new[] { "Admin", "Manager" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task RoleHandler_WithCaseInsensitiveMatch_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<RoleHandler>>();
        var handler = new RoleHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new(ClaimTypes.Role, "admin")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new RolesAuthorizationRequirement(new[] { "Admin" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task RoleHandler_WithMultipleUserRoles_ShouldSucceedIfAnyMatches()
    {
        // Arrange
        var logger = new Mock<ILogger<RoleHandler>>();
        var handler = new RoleHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new(ClaimTypes.Role, "User"),
            new(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new RolesAuthorizationRequirement(new[] { "Admin", "Manager" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task RoleHandler_WithoutRoleClaims_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<RoleHandler>>();
        var handler = new RoleHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new RolesAuthorizationRequirement(new[] { "Admin" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task RoleHandler_WithoutUserIdClaim_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<RoleHandler>>();
        var handler = new RoleHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Admin")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new RolesAuthorizationRequirement(new[] { "Admin" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task RoleHandler_WithNonRolesRequirement_ShouldNotProcess()
    {
        // Arrange
        var logger = new Mock<ILogger<RoleHandler>>();
        var handler = new RoleHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new(ClaimTypes.Role, "Admin")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PermissionRequirement("courses:read");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        // Should not succeed because handler only processes RolesAuthorizationRequirement
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region PremiumSubscriptionHandler Tests

    [Fact]
    public async Task PremiumSubscriptionHandler_WithPremiumTier_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<PremiumSubscriptionHandler>>();
        var handler = new PremiumSubscriptionHandler(logger.Object);

        var claims = new List<Claim>
        {
            new("subscription_tier", "Premium")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PremiumSubscriptionRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task PremiumSubscriptionHandler_WithEliteTier_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<PremiumSubscriptionHandler>>();
        var handler = new PremiumSubscriptionHandler(logger.Object);

        var claims = new List<Claim>
        {
            new("subscription_tier", "Elite")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PremiumSubscriptionRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task PremiumSubscriptionHandler_WithFreeTier_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<PremiumSubscriptionHandler>>();
        var handler = new PremiumSubscriptionHandler(logger.Object);

        var claims = new List<Claim>
        {
            new("subscription_tier", "Free")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PremiumSubscriptionRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task PremiumSubscriptionHandler_WithoutSubscriptionClaim_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<PremiumSubscriptionHandler>>();
        var handler = new PremiumSubscriptionHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new PremiumSubscriptionRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region EmailConfirmedHandler Tests

    [Fact]
    public async Task EmailConfirmedHandler_WithConfirmedEmail_ShouldSucceed()
    {
        // Arrange
        var logger = new Mock<ILogger<EmailConfirmedHandler>>();
        var handler = new EmailConfirmedHandler(logger.Object);

        var claims = new List<Claim>
        {
            new("email_verified", "true")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new EmailConfirmedRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task EmailConfirmedHandler_WithUnconfirmedEmail_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<EmailConfirmedHandler>>();
        var handler = new EmailConfirmedHandler(logger.Object);

        var claims = new List<Claim>
        {
            new("email_verified", "false")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new EmailConfirmedRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task EmailConfirmedHandler_WithInvalidBooleanValue_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<EmailConfirmedHandler>>();
        var handler = new EmailConfirmedHandler(logger.Object);

        var claims = new List<Claim>
        {
            new("email_verified", "invalid")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new EmailConfirmedRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task EmailConfirmedHandler_WithoutEmailVerifiedClaim_ShouldFail()
    {
        // Arrange
        var logger = new Mock<ILogger<EmailConfirmedHandler>>();
        var handler = new EmailConfirmedHandler(logger.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new EmailConfirmedRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion
}
