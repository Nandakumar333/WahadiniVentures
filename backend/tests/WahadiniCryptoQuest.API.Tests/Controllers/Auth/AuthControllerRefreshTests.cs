using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.DAL.Context;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for AuthController token refresh functionality
/// Tests the complete refresh token flow from API to database
/// </summary>
public class AuthControllerRefreshTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _databaseName;

    public AuthControllerRefreshTests(WebApplicationFactory<Program> factory)
    {
        _databaseName = $"TestDb_AuthRefresh_{Guid.NewGuid()}";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing with unique database name
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                    options.EnableSensitiveDataLogging();
                });
            });

            // Use Testing environment which should have rate limiting disabled or very high limits
            builder.UseEnvironment("Testing");
        });

        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    /// <summary>
    /// Helper method to create a user, confirm email, and login to get tokens
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

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        // Confirm email
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        user!.ConfirmEmail();
        await dbContext.SaveChangesAsync();

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
    public async Task RefreshToken_WithValidToken_Returns200OK()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh1@example.com", "Password123!");
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewAccessAndRefreshTokens()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh2@example.com", "Password123!");
        var originalAccessToken = loginResult.AccessToken;
        var originalRefreshToken = loginResult.RefreshToken;

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = originalRefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        var content = await response.Content.ReadAsStringAsync();
        var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(content, _jsonOptions);

        // Assert
        refreshResponse.Should().NotBeNull();
        refreshResponse!.Success.Should().BeTrue();
        refreshResponse.AccessToken.Should().NotBeNullOrEmpty();
        refreshResponse.RefreshToken.Should().NotBeNullOrEmpty();
        refreshResponse.AccessToken.Should().NotBe(originalAccessToken);
        refreshResponse.RefreshToken.Should().NotBe(originalRefreshToken);
        refreshResponse.ExpiresIn.Should().BeGreaterThan(0);
        refreshResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        refreshResponse.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task RefreshToken_RevokesOldRefreshTokenInDatabase()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh3@example.com", "Password123!");
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert - Check that old token is revoked
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var oldToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == loginResult.RefreshToken);

        oldToken.Should().NotBeNull();
        oldToken!.IsRevoked.Should().BeTrue();
        oldToken.RevokedAt.Should().NotBeNull();
        oldToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RefreshToken_SavesNewRefreshTokenToDatabase()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh4@example.com", "Password123!");
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        var content = await response.Content.ReadAsStringAsync();
        var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(content, _jsonOptions);

        // Assert - Check that new token exists in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var newToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshResponse!.RefreshToken);

        newToken.Should().NotBeNull();
        newToken!.IsRevoked.Should().BeFalse();
        newToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        newToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_Returns401Unauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid-token-string-that-does-not-exist"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error");
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_Returns401Unauthorized()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh5@example.com", "Password123!");

        // Manually expire the token in database using reflection
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var token = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == loginResult.RefreshToken);

            // Use reflection to set private property
            var property = typeof(WahadiniCryptoQuest.Core.Entities.RefreshToken)
                .GetProperty("ExpiresAt");
            property!.SetValue(token, DateTime.UtcNow.AddDays(-1));

            await dbContext.SaveChangesAsync();
        }

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error");
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_Returns401Unauthorized()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh6@example.com", "Password123!");

        // Manually revoke the token in database
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var token = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == loginResult.RefreshToken);
            token!.Revoke();
            await dbContext.SaveChangesAsync();
        }

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error");
    }

    [Fact]
    public async Task RefreshToken_RecordsDeviceInfoAndIPAddress()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh7@example.com", "Password123!");
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken,
            DeviceInfo = "Mozilla/5.0 Test Browser",
            IpAddress = "192.168.1.100"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        var content = await response.Content.ReadAsStringAsync();
        var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(content, _jsonOptions);

        // Assert - Check that new token has device info
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var newToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshResponse!.RefreshToken);

        newToken.Should().NotBeNull();
        newToken!.DeviceInfo.Should().Contain("Mozilla");
        newToken.IpAddress.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_WithReusedToken_DetectsTokenRotationAttack()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh8@example.com", "Password123!");
        var originalRefreshToken = loginResult.RefreshToken;

        var firstRefreshRequest = new RefreshTokenRequest
        {
            RefreshToken = originalRefreshToken
        };

        // First refresh (valid)
        await _client.PostAsJsonAsync("/api/auth/refresh", firstRefreshRequest);

        // Try to reuse the same token (should fail - token rotation attack)
        var secondRefreshRequest = new RefreshTokenRequest
        {
            RefreshToken = originalRefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", secondRefreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error");

        // Verify the original token is marked as revoked
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var token = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == originalRefreshToken);
        token!.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshToken_NewAccessTokenHasUpdatedExpirationTime()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh9@example.com", "Password123!");
        await Task.Delay(1000); // Wait 1 second to ensure different expiration

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        var content = await response.Content.ReadAsStringAsync();
        var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(content, _jsonOptions);

        // Assert
        refreshResponse.Should().NotBeNull();
        refreshResponse!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        refreshResponse.ExpiresIn.Should().BeGreaterThan(0);

        // New expiration should be later than original
        var originalExpiration = DateTime.UtcNow.AddSeconds(loginResult.ExpiresIn);
        refreshResponse.ExpiresAt.Should().BeAfter(originalExpiration.AddSeconds(-2)); // Account for timing
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_Returns400BadRequest()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_WithNullToken_Returns400BadRequest()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = null!
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_ValidatesTokenBelongsToCorrectUser()
    {
        // Arrange - Create two users and get their tokens
        var user1Login = await CreateAndLoginUserAsync("refresh10@example.com", "Password123!");
        var user2Login = await CreateAndLoginUserAsync("refresh11@example.com", "Password123!");

        // Try to use user1's token
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = user1Login.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert - Should succeed for correct user
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the token belongs to user1
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var token = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == user1Login.RefreshToken);

        token.Should().NotBeNull();
        token!.User.Email.Should().Be("refresh10@example.com");
    }

    [Fact]
    public async Task RefreshToken_WithMalformedJson_Returns400BadRequest()
    {
        // Arrange
        var malformedJson = "{ invalid json }";
        var content = new StringContent(malformedJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/refresh", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_ResponseShouldContainCorrectHeaders()
    {
        // Arrange
        var loginResult = await CreateAndLoginUserAsync("refresh12@example.com", "Password123!");
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.Headers.Should().NotBeNull();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
