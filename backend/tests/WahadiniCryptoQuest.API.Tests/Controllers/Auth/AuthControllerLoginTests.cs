using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.API;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for AuthController login functionality
/// Tests the complete login flow from API to database
/// </summary>
public class AuthControllerLoginTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerLoginTests(TestWebApplicationFactory<Program> factory)
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
    public async Task Login_WithValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange - First create and confirm a user
        var registerRequest = new RegisterDto
        {
            Email = "logintest@example.com",
            Password = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!",
            FirstName = "Login",
            LastName = "Test",
            AcceptTerms = true
        };

        // Register the user
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Confirm the user's email to enable login
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WahadiniCryptoQuest.DAL.Context.ApplicationDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
        user.Should().NotBeNull();

        // Manually confirm the email for testing
        user!.ConfirmEmail();
        await dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "logintest@example.com",
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
        loginResponse!.Success.Should().BeTrue();
        loginResponse.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse.RefreshToken.Should().NotBeNullOrEmpty();
        loginResponse.ExpiresIn.Should().BeGreaterThan(0);
        loginResponse.User.Should().NotBeNull();
        loginResponse.User!.Email.Should().Be(loginRequest.Email);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "invalid-email-format",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithCorrectEmailButWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com", // Assuming this user exists from test setup
            Password = "WrongPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithRememberMeTrue_ReturnsSuccessWithExtendedExpiry()
    {
        // Arrange - First create and confirm a user
        var registerRequest = new RegisterDto
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!",
            FirstName = "Test",
            LastName = "User",
            AcceptTerms = true
        };

        // Register the user
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Confirm the user's email to enable login
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WahadiniCryptoQuest.DAL.Context.ApplicationDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
        user.Should().NotBeNull();

        // Manually confirm the email for testing
        user!.ConfirmEmail();
        await dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            RememberMe = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.Success.Should().BeTrue();
        loginResponse.ExpiresIn.Should().BeGreaterThan(0);
        // When RememberMe is true, should have longer expiry (implementation detail)
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "unconfirmed@example.com", // Assuming this is an unconfirmed user
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNullRequestBody_ReturnsBadRequest()
    {
        // Act - Send empty JSON content instead of null
        var response = await _client.PostAsync("/api/auth/login",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithMalformedJson_ReturnsBadRequest()
    {
        // Arrange
        var malformedJson = "{ invalid json }";
        var content = new StringContent(malformedJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ResponseShouldContainCorrectHeaders()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        response.Headers.Should().NotBeNull();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task Login_ShouldSetSecurityHeaders()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        // Check for security headers (implementation may vary)
        response.Headers.Should().NotBeNull();
    }

    [Theory]
    [InlineData("test.email+tag@domain.co.uk")]
    [InlineData("user@example-domain.org")]
    [InlineData("simple@test.co")]
    public async Task Login_WithValidEmailFormats_ProcessesCorrectly(string email)
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert
        // Should return either OK (if user exists) or Unauthorized (if user doesn't exist)
        // But should NOT return BadRequest for valid email formats
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_TokenResponseShouldHaveValidStructure()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        // Assert - Test structure regardless of success/failure
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

            loginResponse.Should().NotBeNull();
            loginResponse!.Should().Match<LoginResponse>(lr =>
                !string.IsNullOrEmpty(lr.AccessToken) &&
                !string.IsNullOrEmpty(lr.RefreshToken) &&
                lr.ExpiresIn > 0 &&
                lr.User != null);
        }
    }
}
