using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.Interfaces;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for AuthController email confirmation endpoint
/// Tests end-to-end email confirmation flow with database
/// </summary>
public class AuthControllerConfirmEmailTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IEmailService> _mockEmailService;

    public AuthControllerConfirmEmailTests(WebApplicationFactory<Program> factory)
    {
        _mockEmailService = new Mock<IEmailService>();
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing email service registration
                services.RemoveAll(typeof(IEmailService));
                // Add the mock email service
                services.AddSingleton(_mockEmailService.Object);
                
                // Use consistent database name for tests
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDbEmailConfirmation");
                    options.EnableSensitiveDataLogging();
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_Returns200Ok()
    {
        // Arrange
        var (user, token) = await CreateUserWithEmailVerificationTokenAsync();
        
        var confirmationDto = new EmailConfirmationDto
        {
            UserId = user.Id,
            Token = token.Token
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmationDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("successfully verified");
    }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_MarksEmailAsConfirmedInDatabase()
    {
        // Arrange
        var (user, token) = await CreateUserWithEmailVerificationTokenAsync();
        
        var confirmationDto = new EmailConfirmationDto
        {
            UserId = user.Id,
            Token = token.Token
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmationDto);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_SetsEmailConfirmedAtTimestamp()
    {
        // Arrange
        var (user, token) = await CreateUserWithEmailVerificationTokenAsync();
        
        var confirmationDto = new EmailConfirmationDto
        {
            UserId = user.Id,
            Token = token.Token
        };

        var beforeConfirmation = DateTime.UtcNow;

        // Act
        await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmationDto);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.EmailConfirmedAt.Should().NotBeNull();
        updatedUser.EmailConfirmedAt!.Value.Should().BeCloseTo(beforeConfirmation, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_Returns400BadRequest()
    {
        // Arrange
        var (user, _) = await CreateUserWithEmailVerificationTokenAsync();
        
        var confirmationDto = new EmailConfirmationDto
        {
            UserId = user.Id,
            Token = "invalid-token-that-does-not-exist"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmationDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmEmail_WithExpiredToken_Returns400BadRequest()
    {
        // Arrange
        var (user, token) = await CreateUserWithEmailVerificationTokenAsync(isExpired: true);
        
        var confirmationDto = new EmailConfirmationDto
        {
            UserId = user.Id,
            Token = token.Token
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmationDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmEmail_WithAlreadyUsedToken_Returns400BadRequest()
    {
        // Arrange
        var (user, token) = await CreateUserWithEmailVerificationTokenAsync();
        
        var confirmationDto = new EmailConfirmationDto
        {
            UserId = user.Id,
            Token = token.Token
        };

        // First confirmation - should succeed
        await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmationDto);

        // Act - Second confirmation attempt with same token
        var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmationDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmEmail_WithNonExistentUserId_Returns400BadRequest()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var confirmationDto = new EmailConfirmationDto
        {
            UserId = nonExistentUserId,
            Token = "some-valid-token-format"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmationDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmEmail_MarksTokenAsUsed()
    {
        // Arrange
        var (user, token) = await CreateUserWithEmailVerificationTokenAsync();
        
        var confirmationDto = new EmailConfirmationDto
        {
            UserId = user.Id,
            Token = token.Token
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmationDto);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var updatedToken = await context.EmailVerificationTokens.FindAsync(token.Id);
        updatedToken.Should().NotBeNull();
        updatedToken!.IsUsed.Should().BeTrue();
        updatedToken.UsedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Helper method to create a user with an email verification token for testing
    /// </summary>
    /// <param name="isExpired">Whether the token should be expired</param>
    /// <returns>Tuple of created user and verification token</returns>
    private async Task<(User user, EmailVerificationToken token)> CreateUserWithEmailVerificationTokenAsync(bool isExpired = false)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create a test user
        var user = User.Create(
            email: $"confirm-test-{Guid.NewGuid()}@example.com",
            firstName: "Test",
            lastName: "User",
            passwordHash: "hashedpassword"
        );

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Create a verification token
        var tokenValue = Guid.NewGuid().ToString();
        
        EmailVerificationToken token;
        if (isExpired)
        {
            token = EmailVerificationToken.CreateExpiredToken(
                userId: user.Id,
                tokenValue: tokenValue,
                hoursExpiredAgo: 1
            );
        }
        else
        {
            token = EmailVerificationToken.CreateWithToken(
                userId: user.Id,
                tokenValue: tokenValue,
                expirationHours: 24
            );
        }

        context.EmailVerificationTokens.Add(token);
        await context.SaveChangesAsync();

        return (user, token);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
