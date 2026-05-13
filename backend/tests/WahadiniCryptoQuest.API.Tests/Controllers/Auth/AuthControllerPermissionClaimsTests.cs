using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.API;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.Core.Entities;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for JWT permission claims in authentication tokens
/// Verifies that user permissions are correctly embedded in JWT tokens
/// </summary>
public class AuthControllerPermissionClaimsTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerPermissionClaimsTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    [Fact]
    public async Task Login_WithUserHavingFreeRole_ShouldIncludeFreePermissionsInJWT()
    {
        // Arrange - Create user with Free role
        var registerRequest = new RegisterDto
        {
            Email = "freeuser@example.com",
            Password = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!",
            FirstName = "Free",
            LastName = "User",
            AcceptTerms = true
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
            user.Should().NotBeNull();

            // Confirm email
            user!.ConfirmEmail();

            // Assign Free role
            var freeRole = await dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == "Free");
            if (freeRole != null)
            {
                var userRole = UserRole.Create(user, freeRole, null);
                dbContext.Set<UserRole>().Add(userRole);
            }

            await dbContext.SaveChangesAsync();
        }

        var loginRequest = new LoginRequest
        {
            Email = "freeuser@example.com",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();

        // Decode JWT and verify permission claims
        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadJwtToken(loginResponse.AccessToken);

        var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").ToList();

        // Free role should have basic permissions
        permissionClaims.Should().NotBeEmpty("Free role should have assigned permissions");

        // Verify some expected Free role permissions
        var permissionValues = permissionClaims.Select(c => c.Value).ToList();
        permissionValues.Should().Contain("courses:view", "Free users should have courses:view permission");
        permissionValues.Should().Contain("lessons:view", "Free users should have lessons:view permission");
        permissionValues.Should().Contain("profile:read", "Free users should have profile:read permission");
    }

    [Fact]
    public async Task Login_WithUserHavingPremiumRole_ShouldIncludePremiumPermissionsInJWT()
    {
        // Arrange - Create user with Premium role
        var registerRequest = new RegisterDto
        {
            Email = "premiumuser@example.com",
            Password = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!",
            FirstName = "Premium",
            LastName = "User",
            AcceptTerms = true
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
            user.Should().NotBeNull();

            // Confirm email
            user!.ConfirmEmail();

            // Assign Premium role
            var premiumRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Premium");
            if (premiumRole != null)
            {
                var userRole = UserRole.Create(user, premiumRole, DateTime.UtcNow.AddYears(1));
                dbContext.Set<UserRole>().Add(userRole);
            }

            await dbContext.SaveChangesAsync();
        }

        var loginRequest = new LoginRequest
        {
            Email = "premiumuser@example.com",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();

        // Decode JWT and verify permission claims
        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadJwtToken(loginResponse.AccessToken);

        var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").ToList();

        // Premium role should have more permissions than Free
        permissionClaims.Should().NotBeEmpty("Premium role should have assigned permissions");
        permissionClaims.Count.Should().BeGreaterThan(5, "Premium role should have multiple permissions");

        // Verify some expected Premium role permissions
        var permissionValues = permissionClaims.Select(c => c.Value).ToList();
        permissionValues.Should().Contain("courses:complete", "Premium users should have courses:complete permission");
        permissionValues.Should().Contain("tasks:submit", "Premium users should have tasks:submit permission");
        permissionValues.Should().Contain("rewards:claim", "Premium users should have rewards:claim permission");
    }

    [Fact]
    public async Task Login_WithUserHavingAdminRole_ShouldIncludeAllAdminPermissionsInJWT()
    {
        // Arrange - Create user with Admin role
        var registerRequest = new RegisterDto
        {
            Email = "adminuser@example.com",
            Password = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!",
            FirstName = "Admin",
            LastName = "User",
            AcceptTerms = true
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
            user.Should().NotBeNull();

            // Confirm email
            user!.ConfirmEmail();

            // Assign Admin role
            var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var userRole = UserRole.Create(user, adminRole, null);
                dbContext.Set<UserRole>().Add(userRole);
            }

            await dbContext.SaveChangesAsync();
        }

        var loginRequest = new LoginRequest
        {
            Email = "adminuser@example.com",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();

        // Decode JWT and verify permission claims
        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadJwtToken(loginResponse.AccessToken);

        var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").ToList();

        // Admin role should have extensive permissions
        permissionClaims.Should().NotBeEmpty("Admin role should have assigned permissions");
        permissionClaims.Count.Should().BeGreaterThan(20, "Admin role should have many permissions");

        // Verify some expected Admin role permissions
        var permissionValues = permissionClaims.Select(c => c.Value).ToList();
        permissionValues.Should().Contain("users:edit", "Admin should have users:edit permission");
        permissionValues.Should().Contain("courses:manage", "Admin should have courses:manage permission");
        permissionValues.Should().Contain("tasks:review", "Admin should have tasks:review permission");
        permissionValues.Should().Contain("analytics:view", "Admin should have analytics:view permission");
        permissionValues.Should().Contain("settings:manage", "Admin should have settings:manage permission");
    }

    [Fact]
    public async Task Login_WithUserWithoutRole_ShouldHaveEmptyPermissionsInJWT()
    {
        // Arrange - Create user without role
        var registerRequest = new RegisterDto
        {
            Email = "noroleuser@example.com",
            Password = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!",
            FirstName = "NoRole",
            LastName = "User",
            AcceptTerms = true
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
            user.Should().NotBeNull();

            // Confirm email but DON'T assign any role
            user!.ConfirmEmail();
            await dbContext.SaveChangesAsync();
        }

        var loginRequest = new LoginRequest
        {
            Email = "noroleuser@example.com",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();

        // Decode JWT and verify NO permission claims
        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadJwtToken(loginResponse.AccessToken);

        var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").ToList();

        // User without role should have no permissions
        permissionClaims.Should().BeEmpty("User without role should have no permissions");
    }

    [Fact]
    public async Task RefreshToken_ShouldReloadPermissionsFromDatabase()
    {
        // Arrange - Create user with Free role
        var registerRequest = new RegisterDto
        {
            Email = "refreshpermtest@example.com",
            Password = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!",
            FirstName = "Refresh",
            LastName = "Test",
            AcceptTerms = true
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
            user!.ConfirmEmail();

            // Initially assign Free role
            var freeRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Free");
            if (freeRole != null)
            {
                var userRole = UserRole.Create(user, freeRole, null);
                dbContext.Set<UserRole>().Add(userRole);
            }

            await dbContext.SaveChangesAsync();
        }

        // Login to get initial tokens
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "refreshpermtest@example.com", Password = "ValidPassword123!", RememberMe = false },
            _jsonOptions);
        var loginResult = JsonSerializer.Deserialize<LoginResponse>(
            await loginResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Verify initial Free permissions
        var jwtHandler = new JwtSecurityTokenHandler();
        var initialToken = jwtHandler.ReadJwtToken(loginResult!.AccessToken);
        var initialPermissions = initialToken.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        initialPermissions.Should().NotContain("users:edit", "Free role should not have admin permissions");

        // Upgrade user to Admin role
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);

            // Remove old role
            var oldRoles = await dbContext.Set<UserRole>().Where(ur => ur.UserId == user!.Id).ToListAsync();
            dbContext.Set<UserRole>().RemoveRange(oldRoles);

            // Add Admin role
            var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var userRole = UserRole.Create(user!, adminRole, null);
                dbContext.Set<UserRole>().Add(userRole);
            }

            await dbContext.SaveChangesAsync();
        }

        // Act - Refresh token
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = loginResult.RefreshToken },
            _jsonOptions);

        // Assert - New token should have Admin permissions
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = JsonSerializer.Deserialize<RefreshTokenResponse>(
            await refreshResponse.Content.ReadAsStringAsync(), _jsonOptions);

        var refreshedToken = jwtHandler.ReadJwtToken(refreshResult!.AccessToken);
        var refreshedPermissions = refreshedToken.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

        refreshedPermissions.Should().Contain("users:edit", "Upgraded to Admin should have users:edit permission");
        refreshedPermissions.Should().Contain("settings:manage", "Upgraded to Admin should have settings:manage permission");
        refreshedPermissions.Count.Should().BeGreaterThan(initialPermissions.Count, "Admin should have more permissions than Free");
    }

    [Fact]
    public async Task JWT_PermissionClaimsShouldFollowResourceActionFormat()
    {
        // Arrange - Create user with any role
        var registerRequest = new RegisterDto
        {
            Email = "formattest@example.com",
            Password = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!",
            FirstName = "Format",
            LastName = "Test",
            AcceptTerms = true
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
            user!.ConfirmEmail();

            var freeRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Free");
            if (freeRole != null)
            {
                var userRole = UserRole.Create(user, freeRole, null);
                dbContext.Set<UserRole>().Add(userRole);
            }

            await dbContext.SaveChangesAsync();
        }

        // Act
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "formattest@example.com", Password = "ValidPassword123!", RememberMe = false },
            _jsonOptions);

        // Assert
        var loginResult = JsonSerializer.Deserialize<LoginResponse>(
            await loginResponse.Content.ReadAsStringAsync(), _jsonOptions);

        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadJwtToken(loginResult!.AccessToken);
        var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

        // All permissions should follow "resource:action" format
        foreach (var permission in permissionClaims)
        {
            permission.Should().Contain(":", $"Permission '{permission}' should follow 'resource:action' format");
            var parts = permission.Split(':');
            parts.Should().HaveCount(2, $"Permission '{permission}' should have exactly one colon");
            parts[0].Should().NotBeNullOrWhiteSpace("Resource part should not be empty");
            parts[1].Should().NotBeNullOrWhiteSpace("Action part should not be empty");
        }
    }
}
