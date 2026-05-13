using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.DAL.Context;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for logout endpoint
/// Tests token revocation and authentication requirements
/// </summary>
public class AuthControllerLogoutTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerLogoutTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Helper method to create and login a user
    /// </summary>
    private async Task<LoginResponse> CreateAndLoginUserAsync(string email, string password)
    {
        // Register
        var registerRequest = new RegisterDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User",
            AcceptTerms = true
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Mark as verified
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.ConfirmEmail();
                await dbContext.SaveChangesAsync();
            }
        }

        // Login
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password,
            RememberMe = false
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await loginResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions)!;
    }

    [Fact]
    public async Task Logout_WithValidToken_RevokesRefreshTokenInDatabase()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout1@example.com", "Password123!");

        // Add Authorization header
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert - Should succeed
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the token is revoked in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var token = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == loginResult.RefreshToken);

        token.Should().NotBeNull();
        token!.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().NotBeNull();
        token.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Logout_WithValidToken_Returns200OK()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout2@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LogoutResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Contain("successfully");
    }

    [Fact]
    public async Task Logout_RequiresAuthentication_Returns401Unauthorized()
    {
        // Arrange - No authorization header
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "some-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithAlreadyRevokedToken_ReturnsSuccess()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout4@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act - Logout twice
        var firstResponse = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert - Both should succeed (idempotent operation)
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_WithInvalidRefreshToken_ReturnsSuccess()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout5@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "invalid-non-existent-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert - Should succeed (security: don't reveal if token exists)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_WithMissingRefreshToken_Returns400BadRequest()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout6@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "" // Empty token
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error");
    }

    [Fact]
    public async Task Logout_LogsLogoutActivity()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout7@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Note: In a real scenario, you would verify logs through a logging provider
        // This test serves as documentation that logging should occur
    }

    [Fact]
    public async Task Logout_ClearsConcurrentSessions()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout8@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act - Logout
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify in database that the token is revoked
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var token = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == loginResult.RefreshToken);

        token.Should().NotBeNull();
        token!.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Logout_HandlesInvalidModelState()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout9@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        // Act - Send request with empty/null RefreshToken (invalid model state)
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "" // Empty refresh token should trigger validation error
        };
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_HandlesDbConcurrencyErrors()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout10@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act - Attempt concurrent logouts
        var task1 = _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);
        var task2 = _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        await Task.WhenAll(task1, task2);

        var response1 = await task1;
        var response2 = await task2;

        // Assert - Both should succeed (one will revoke, one will find already revoked)
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_RevokedTokenCannotBeUsedToRefresh()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("logout3@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act - Logout
        await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Try to use the revoked token to refresh
        _client.DefaultRequestHeaders.Authorization = null; // Remove auth header for refresh
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert - Should fail
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await refreshResponse.Content.ReadAsStringAsync();
        content.Should().Contain("error");
        content.ToLower().Should().Contain("invalid");
    }
}

/// <summary>
/// Logout request DTO
/// </summary>
public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Logout response DTO
/// </summary>
public class LogoutResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
