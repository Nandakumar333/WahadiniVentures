using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.Interfaces;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using BCrypt.Net;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for AuthController password reset endpoints
/// Tests end-to-end password reset flow with database and email service
/// </summary>
public class AuthControllerPasswordResetTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly string _databaseName;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerPasswordResetTests(WebApplicationFactory<Program> factory)
    {
        _mockEmailService = new Mock<IEmailService>();
        _databaseName = $"TestDb_AuthPasswordReset_{Guid.NewGuid()}";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                    options.EnableSensitiveDataLogging();
                });

                // Replace email service with mock
                services.RemoveAll<IEmailService>();
                services.AddSingleton(_mockEmailService.Object);
            });

            // Use Testing environment to disable rate limiting
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

    #region Forgot Password Tests

    [Fact]
    public async Task POST_PasswordResetRequest_WithValidEmail_ShouldReturn200OK()
    {
        // Arrange
        await CreateTestUserAsync("resetuser@example.com", "OldPassword123!");

        var request = new PasswordResetRequest
        {
            Email = "resetuser@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadFromJsonAsync<PasswordResetResponse>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Message.Should().Contain("password reset link has been sent");
    }

    [Fact]
    public async Task POST_PasswordResetRequest_WithValidEmail_ShouldCreateTokenInDatabase()
    {
        // Arrange
        var email = "tokentest@example.com";
        await CreateTestUserAsync(email, "OldPassword123!");

        var request = new PasswordResetRequest { Email = email };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify token was created in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        user.Should().NotBeNull();

        var token = await dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user!.Id && !t.IsUsed);

        token.Should().NotBeNull();
        token!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        token.IsUsed.Should().BeFalse();
        token.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task POST_PasswordResetRequest_WithValidEmail_ShouldSendEmailWithResetLink()
    {
        // Arrange
        var email = "emailtest@example.com";
        await CreateTestUserAsync(email, "OldPassword123!");

        var request = new PasswordResetRequest { Email = email };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify email service was called
        _mockEmailService.Verify(
            x => x.SendPasswordResetEmailAsync(
                email,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task POST_PasswordResetRequest_WithNonExistentEmail_ShouldReturn200OK()
    {
        // Arrange - Security: Don't reveal if email exists
        var request = new PasswordResetRequest
        {
            Email = "nonexistent@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/request", request);

        // Assert - Always return 200 to prevent email enumeration
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadFromJsonAsync<PasswordResetResponse>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POST_PasswordResetRequest_WithExistingToken_ShouldInvalidatePreviousToken()
    {
        // Arrange
        var email = "invalidatetest@example.com";
        var userId = await CreateTestUserAsync(email, "OldPassword123!");

        // Create an existing token
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var oldToken = PasswordResetToken.Create(userId, DateTime.UtcNow.AddHours(1));
            // Force the token value for testing
            typeof(PasswordResetToken).GetProperty("Token")!.SetValue(oldToken, "old-token-12345");
            dbContext.PasswordResetTokens.Add(oldToken);
            await dbContext.SaveChangesAsync();
        }

        var request = new PasswordResetRequest { Email = email };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify old token was revoked and new token created
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var allTokens = await dbContext.PasswordResetTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            // Should have 2 tokens: old (revoked) and new (active)
            allTokens.Should().HaveCount(2);

            var oldToken = allTokens.First(t => t.Token == "old-token-12345");
            oldToken.IsUsed.Should().BeTrue(); // Old token should be marked as used

            var newToken = allTokens.First(t => t.Token != "old-token-12345");
            newToken.IsUsed.Should().BeFalse(); // New token should be active
        }
    }

    [Fact]
    public async Task POST_PasswordResetRequest_WithInvalidEmail_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new PasswordResetRequest
        {
            Email = "invalid-email-format"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Reset Password Confirm Tests

    [Fact]
    public async Task POST_PasswordResetConfirm_WithValidToken_ShouldReturn200OK()
    {
        // Arrange
        var email = "confirmtest@example.com";
        var userId = await CreateTestUserAsync(email, "OldPassword123!");
        var token = await CreatePasswordResetTokenAsync(userId);

        var request = new PasswordResetConfirmRequest
        {
            Token = token,
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadFromJsonAsync<PasswordResetResponse>();
        responseContent.Should().NotBeNull();
        responseContent!.Success.Should().BeTrue();
        responseContent.Message.Should().Contain("reset successfully");
    }

    [Fact]
    public async Task POST_PasswordResetConfirm_WithValidToken_ShouldUpdatePasswordInDatabase()
    {
        // Arrange
        var email = "passwordupdatetest@example.com";
        var oldPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";
        var userId = await CreateTestUserAsync(email, oldPassword);
        var token = await CreatePasswordResetTokenAsync(userId);

        var request = new PasswordResetConfirmRequest
        {
            Token = token,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify password was updated in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.Users.FindAsync(userId);

        user.Should().NotBeNull();
        // Verify new password works
        BCrypt.Net.BCrypt.Verify(newPassword, user!.PasswordHash).Should().BeTrue();
        // Verify old password no longer works
        BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash).Should().BeFalse();
    }

    [Fact]
    public async Task POST_PasswordResetConfirm_WithValidToken_ShouldMarkTokenAsUsed()
    {
        // Arrange
        var email = "tokenusedtest@example.com";
        var userId = await CreateTestUserAsync(email, "OldPassword123!");
        var token = await CreatePasswordResetTokenAsync(userId);

        var request = new PasswordResetConfirmRequest
        {
            Token = token,
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify token was marked as used
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var resetToken = await dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        resetToken.Should().NotBeNull();
        resetToken!.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task POST_PasswordResetConfirm_WithValidToken_ShouldRevokeAllRefreshTokens()
    {
        // Arrange
        var email = "revoketest@example.com";
        var userId = await CreateTestUserAsync(email, "OldPassword123!");
        var token = await CreatePasswordResetTokenAsync(userId);

        // Create some refresh tokens
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var refreshToken1 = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(7));
            var refreshToken2 = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(7));
            dbContext.RefreshTokens.AddRange(refreshToken1, refreshToken2);
            await dbContext.SaveChangesAsync();
        }

        var request = new PasswordResetConfirmRequest
        {
            Token = token,
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify all refresh tokens were revoked
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var refreshTokens = await dbContext.RefreshTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            refreshTokens.Should().HaveCount(2);
            refreshTokens.Should().OnlyContain(t => t.IsRevoked);
        }
    }

    [Fact]
    public async Task POST_PasswordResetConfirm_WithInvalidToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        var request = new PasswordResetConfirmRequest
        {
            Token = "invalid-token-12345",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_PasswordResetConfirm_WithExpiredToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        var email = "expiredtest@example.com";
        var userId = await CreateTestUserAsync(email, "OldPassword123!");

        // Create a valid token first, then manually expire it in the database
        string expiredToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Create valid token
            var resetToken = PasswordResetToken.Create(userId, DateTime.UtcNow.AddHours(1));
            dbContext.PasswordResetTokens.Add(resetToken);
            await dbContext.SaveChangesAsync();
            expiredToken = resetToken.Token;

            // Now manually set it to expired using EF Core's tracking
            var tokenEntry = dbContext.Entry(resetToken);
            tokenEntry.Property("ExpiresAt").CurrentValue = DateTime.UtcNow.AddHours(-1);
            await dbContext.SaveChangesAsync();
        }

        var request = new PasswordResetConfirmRequest
        {
            Token = expiredToken,
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_PasswordResetConfirm_WithUsedToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        var email = "usedtokentest@example.com";
        var userId = await CreateTestUserAsync(email, "OldPassword123!");

        // Create used token
        string usedToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var resetToken = PasswordResetToken.Create(userId, DateTime.UtcNow.AddHours(1));
            resetToken.MarkAsUsed(); // Mark as used
            dbContext.PasswordResetTokens.Add(resetToken);
            await dbContext.SaveChangesAsync();
            usedToken = resetToken.Token;
        }

        var request = new PasswordResetConfirmRequest
        {
            Token = usedToken,
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_PasswordResetConfirm_AfterReset_UserCanLoginWithNewPassword()
    {
        // Arrange
        var email = "logintest@example.com";
        var oldPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";
        var userId = await CreateTestUserAsync(email, oldPassword);
        var token = await CreatePasswordResetTokenAsync(userId);

        var resetRequest = new PasswordResetConfirmRequest
        {
            Token = token,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        // Act - Reset password
        var resetResponse = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", resetRequest);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Try to login with new password
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = newPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        loginContent.Should().NotBeNull();
        loginContent!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task POST_PasswordResetConfirm_AfterReset_OldPasswordShouldNotWork()
    {
        // Arrange
        var email = "oldpasswordtest@example.com";
        var oldPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";
        var userId = await CreateTestUserAsync(email, oldPassword);
        var token = await CreatePasswordResetTokenAsync(userId);

        var resetRequest = new PasswordResetConfirmRequest
        {
            Token = token,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        // Act - Reset password
        var resetResponse = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", resetRequest);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Try to login with old password
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = oldPassword // Using old password
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_PasswordResetConfirm_WithWeakPassword_ShouldReturn400BadRequest()
    {
        // Arrange
        var email = "weakpasswordtest@example.com";
        var userId = await CreateTestUserAsync(email, "OldPassword123!");
        var token = await CreatePasswordResetTokenAsync(userId);

        var request = new PasswordResetConfirmRequest
        {
            Token = token,
            NewPassword = "weak", // Too short, missing requirements
            ConfirmPassword = "weak"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateTestUserAsync(string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = User.Create(
            email,
            BCrypt.Net.BCrypt.HashPassword(password),
            "Test",
            "User"
        );

        user.ConfirmEmail(); // Confirm email so user can login

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user.Id;
    }

    private async Task<string> CreatePasswordResetTokenAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var token = PasswordResetToken.Create(userId, DateTime.UtcNow.AddHours(1));

        dbContext.PasswordResetTokens.Add(token);
        await dbContext.SaveChangesAsync();

        return token.Token;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    #endregion
}
