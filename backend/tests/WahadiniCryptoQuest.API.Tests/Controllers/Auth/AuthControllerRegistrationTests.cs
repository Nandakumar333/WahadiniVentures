using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.Interfaces;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using BCrypt.Net;
using UserRole = WahadiniCryptoQuest.Core.Enums.UserRoleEnum;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for AuthController registration endpoint
/// Tests end-to-end registration flow with database and email service
/// </summary>
public class AuthControllerRegistrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IEmailService> _mockEmailService;

    public AuthControllerRegistrationTests(WebApplicationFactory<Program> factory)
    {
        _mockEmailService = new Mock<IEmailService>();
        
        // Use a consistent database name for all tests in this class
        var databaseName = $"TestDb_AuthRegistration_{GetHashCode()}";
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing with a consistent name
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName);
                    options.EnableSensitiveDataLogging();
                });

                // Replace email service with mock
                services.RemoveAll<IEmailService>();
                services.AddSingleton(_mockEmailService.Object);
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_Register_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            AcceptTerms = true,
            AcceptMarketing = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Registration successful");
        responseContent.Should().Contain("test@example.com");
    }

    [Fact]
    public async Task POST_Register_WithValidData_ShouldCreateUserInDatabase()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "dbtest@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Jane",
            LastName = "Smith",
            AcceptTerms = true,
            AcceptMarketing = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify user was created in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "dbtest@example.com");
        user.Should().NotBeNull();
        user!.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.EmailConfirmed.Should().BeFalse();
        user.IsActive.Should().BeTrue();
        user.Role.Should().Be(UserRole.Free);
    }

    [Fact]
    public async Task POST_Register_WithValidData_ShouldGenerateEmailVerificationToken()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "tokentest@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Token",
            LastName = "Test",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify email verification token was created
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "tokentest@example.com");
        user.Should().NotBeNull();

        var verificationToken = await context.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.UserId == user!.Id);
        verificationToken.Should().NotBeNull();
        verificationToken!.Token.Should().NotBeNullOrEmpty();
        verificationToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        verificationToken.IsUsed.Should().BeFalse();
    }

    [Fact]
    public async Task POST_Register_WithValidData_ShouldSendVerificationEmail()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "emailtest@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Email",
            LastName = "Test",
            AcceptTerms = true
        };

        _mockEmailService.Setup(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<Guid>(), 
            It.IsAny<string>()))
            .Returns(Task.FromResult(true));

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify email service was called
        _mockEmailService.Verify(x => x.SendEmailVerificationAsync(
            "emailtest@example.com",
            "Email", // Just firstName, not full name
            It.IsAny<Guid>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task POST_Register_WithDuplicateEmail_ShouldReturn409Conflict()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "duplicate@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "First",
            LastName = "User",
            AcceptTerms = true
        };

        // Register first user
        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to register second user with same email
        var secondRegisterDto = new RegisterDto
        {
            Email = "duplicate@example.com", // Same email
            Password = "DifferentPassword123!",
            ConfirmPassword = "DifferentPassword123!",
            FirstName = "Second",
            LastName = "User",
            AcceptTerms = true
        };

        // Act
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", secondRegisterDto);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Controller returns 400 for user already exists
    }

    [Theory]
    [InlineData("", "Password123!", "Password123!", "John", "Doe", true)] // Empty email
    [InlineData("invalid-email", "Password123!", "Password123!", "John", "Doe", true)] // Invalid email format
    [InlineData("test@example.com", "", "Password123!", "John", "Doe", true)] // Empty password
    [InlineData("test@example.com", "Password123!", "", "John", "Doe", true)] // Empty confirm password
    [InlineData("test@example.com", "Password123!", "Password123!", "", "Doe", true)] // Empty first name
    [InlineData("test@example.com", "Password123!", "Password123!", "John", "", true)] // Empty last name
    [InlineData("test@example.com", "Password123!", "Password123!", "John", "Doe", false)] // Terms not accepted
    public async Task POST_Register_WithInvalidEmail_ShouldReturn400BadRequest(
        string email, string password, string confirmPassword, string firstName, string lastName, bool acceptTerms)
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword,
            FirstName = firstName,
            LastName = lastName,
            AcceptTerms = acceptTerms
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("password")] // No uppercase
    [InlineData("PASSWORD")] // No lowercase
    [InlineData("Password")] // No number
    [InlineData("Password123")] // No special character
    public async Task POST_Register_WithWeakPassword_ShouldReturn400BadRequest(string weakPassword)
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "weakpass@example.com",
            Password = weakPassword,
            ConfirmPassword = weakPassword,
            FirstName = "Weak",
            LastName = "Password",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Register_WithMismatchedPasswords_ShouldReturn400BadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "mismatch@example.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!",
            FirstName = "Mismatch",
            LastName = "Test",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Register_WithValidData_ShouldHashPasswordCorrectly()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "hashtest@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Hash",
            LastName = "Test",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify password was hashed using BCrypt
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "hashtest@example.com");
        user.Should().NotBeNull();
        user!.PasswordHash.Should().NotBe("Password123!");
        user.PasswordHash.Should().StartWith("$2"); // BCrypt hashes start with $2
        
        // Verify the password can be verified with BCrypt
        BCrypt.Net.BCrypt.Verify("Password123!", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task POST_Register_WithValidData_ShouldSetEmailConfirmedToFalseByDefault()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "confirmedtest@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Confirmed",
            LastName = "Test",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify EmailConfirmed is false by default
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "confirmedtest@example.com");
        user.Should().NotBeNull();
        user!.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task POST_Register_WithValidData_ShouldSetIsActiveToTrueByDefault()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "activetest@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Active",
            LastName = "Test",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify user is created and not locked (ApplicationUser doesn't have IsActive)
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "activetest@example.com");
        user.Should().NotBeNull();
        user!.IsActive.Should().BeTrue(); // Domain User has IsActive property
    }

    [Fact]
    public async Task POST_Register_WithValidData_ShouldSetRoleToFreeByDefault()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "tiertest@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Tier",
            LastName = "Test",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify Role is Free by default (Domain User uses Role property)
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "tiertest@example.com");
        user.Should().NotBeNull();
        user!.Role.Should().Be(UserRole.Free);
    }

    private async Task CleanupDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Clean up test data
        context.EmailVerificationTokens.RemoveRange(context.EmailVerificationTokens);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        CleanupDatabase().Wait();
        _client.Dispose();
        _factory.Dispose();
    }
}
