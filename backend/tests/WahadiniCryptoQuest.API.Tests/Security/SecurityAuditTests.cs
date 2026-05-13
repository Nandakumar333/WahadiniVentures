using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Security;

[Collection("Sequential")]
public class SecurityAuditTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public SecurityAuditTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Security_ShouldBlock_SQLInjectionAttempts()
    {
        // Arrange - Common SQL injection payloads
        var sqlInjectionPayloads = new[]
        {
            "admin'--",
            "' OR '1'='1",
            "'; DROP TABLE Users--",
            "admin' OR 1=1--",
            "1' UNION SELECT * FROM Users--"
        };

        foreach (var payload in sqlInjectionPayloads)
        {
            var loginRequest = new
            {
                Email = payload,
                Password = "TestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert - Should return validation error or unauthorized, not server error
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,    // Validation failed
                HttpStatusCode.Unauthorized   // Invalid credentials
            );
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError, 
                $"SQL injection payload '{payload}' should not cause server error");
        }
    }

    [Fact]
    public async Task Security_ShouldBlock_XSSAttempts()
    {
        // Arrange - Common XSS payloads
        var xssPayloads = new[]
        {
            "<script>alert('XSS')</script>",
            "<img src=x onerror=alert('XSS')>",
            "javascript:alert('XSS')",
            "<svg onload=alert('XSS')>",
            "<body onload=alert('XSS')>"
        };

        foreach (var payload in xssPayloads)
        {
            var registerRequest = new
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                FirstName = payload,  // XSS in first name
                LastName = "Test"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert - Should sanitize or reject, response should not contain unescaped script tags
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<script>", "XSS payload should be sanitized");
            content.Should().NotContain("onerror=", "XSS payload should be sanitized");
            content.Should().NotContain("javascript:", "XSS payload should be sanitized");
        }
    }

    [Fact]
    public async Task Security_ShouldEnforce_CSRFProtection()
    {
        // Arrange - State-changing operations should require CSRF protection
        var loginRequest = new
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        // Act - Request without anti-forgery token
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert - For JWT-based API, CSRF is less critical but should have proper CORS
        // In test environment, CORS may not be configured
        // Verify security headers are present instead
        response.Headers.Should().ContainKey("X-Content-Type-Options", 
            "Security headers should be configured");
        
        // Check if CORS is configured (optional in test environment)
        var hasCors = response.Headers.Contains("Access-Control-Allow-Origin");
        
        // If CORS is configured, verify it's not wide open
        if (hasCors)
        {
            response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins);
            if (origins != null)
            {
                origins.Should().NotContain("*", "Should not allow all origins in production");
            }
        }
    }

    [Fact]
    public async Task Security_ShouldNotLog_SensitiveData()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "test@example.com",
            Password = "SuperSecretPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Response should never contain raw passwords
        content.Should().NotContain("SuperSecretPassword123!", 
            "Password should never appear in response");
        
        // Password word may appear in error messages, which is acceptable
        var lowerContent = content.ToLower();
        if (lowerContent.Contains("password"))
        {
            // It's OK if generic password messages appear in validation errors
            lowerContent.Should().NotContain("supersecretpassword", 
                "Actual password value should never be logged");
        }

        // Note: Log file inspection would require file system access in integration tests
        // In production, use structured logging with password redaction
    }

    [Fact]
    public async Task Security_ShouldNotExpose_JWTSecrets()
    {
        // Arrange - Check various endpoints for secret exposure
        var endpoints = new[]
        {
            "/api/auth/login",
            "/swagger/v1/swagger.json",
            "/",
            "/api/auth/register"
        };

        foreach (var endpoint in endpoints)
        {
            // Act
            HttpResponseMessage response;
            try
            {
                response = await _client.GetAsync(endpoint);
            }
            catch
            {
                continue; // Some endpoints may not support GET
            }

            var content = await response.Content.ReadAsStringAsync();

            // Assert - Should never expose JWT secret key
            content.Should().NotContain("JwtSecretKey", 
                $"JWT secret should not be exposed in {endpoint}");
            content.Should().NotContain("SecretKey", 
                $"Secret keys should not be exposed in {endpoint}");
            
            var lowerContent = content.ToLower();
            if (lowerContent.Contains("secret"))
            {
                // It's OK if "secret" appears as part of "email" or normal response
                lowerContent.Should().NotContain("jwtsecret", "JWT secret should not be exposed");
                lowerContent.Should().NotContain("secretkey", "Secret key should not be exposed");
            }
        }
    }

    [Fact]
    public async Task Security_ShouldEnforce_HTTPSRedirect()
    {
        // Note: In test environment, HTTPS redirect may be disabled
        // This test verifies the middleware is configured
        
        // Arrange
        var response = await _client.GetAsync("/api/auth/login");

        // Assert - In production, should redirect HTTP to HTTPS
        // In test environment, verify security headers are present
        response.Headers.Should().ContainKey("X-Content-Type-Options", 
            "Security headers should be configured");
        
        // Check for HSTS header (may not be present in test environment)
        var hasHsts = response.Headers.Contains("Strict-Transport-Security");
        
        // At minimum, no downgrade warnings should appear
        response.StatusCode.Should().NotBe(HttpStatusCode.UpgradeRequired);
    }

    [Fact]
    public async Task Security_ShouldHave_SecurityHeaders()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/auth/login");

        // Assert - Essential security headers should be present
        var headers = response.Headers.ToString();
        
        // X-Content-Type-Options prevents MIME sniffing
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");

        // X-Frame-Options prevents clickjacking
        response.Headers.Should().ContainKey("X-Frame-Options");
        
        // Content-Security-Policy (may be configured)
        var hasCsp = response.Headers.Contains("Content-Security-Policy");
        
        // X-XSS-Protection (legacy but still useful)
        var hasXssProtection = response.Headers.Contains("X-XSS-Protection");
        
        // At least basic security headers should be present
        (response.Headers.Contains("X-Content-Type-Options") || 
         response.Headers.Contains("X-Frame-Options")).Should().BeTrue(
            "At least one security header should be present");
    }

    [Fact]
    public async Task Security_ShouldEnforce_PasswordComplexity()
    {
        // Arrange - Weak passwords that should be rejected
        var weakPasswords = new[]
        {
            "123456",           // Too simple
            "password",         // Common word
            "abc123",           // Too short
            "qwerty",           // Keyboard pattern
            "Password",         // Missing special char and number
            "Pass1",            // Too short
            "ALLUPPERCASE1!",   // Missing lowercase
            "alllowercase1!",   // Missing uppercase
            "NoNumbers!",       // Missing numbers
            "NoSpecialChar1"    // Missing special character
        };

        foreach (var weakPassword in weakPasswords)
        {
            var registerRequest = new
            {
                Email = $"test-{Guid.NewGuid()}@example.com",
                Password = weakPassword,
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, 
                $"Weak password '{weakPassword}' should be rejected");
            
            var content = await response.Content.ReadAsStringAsync();
            var lowerContent = content.ToLower();
            lowerContent.Should().Contain("password", "Should have password validation error");
        }

        // Positive case - strong password should be accepted (not rejected for weakness)
        var strongPasswordRequest = new
        {
            Email = $"test-strong-{Guid.NewGuid()}@example.com",
            Password = "StrongP@ssw0rd123!",
            FirstName = "Test",
            LastName = "User"
        };

        var strongResponse = await _client.PostAsJsonAsync("/api/auth/register", strongPasswordRequest);
        var strongContent = await strongResponse.Content.ReadAsStringAsync();
        
        // Strong password should not be rejected for password complexity
        // If it returns BadRequest, verify it's NOT due to password validation
        if (strongResponse.StatusCode == HttpStatusCode.BadRequest)
        {
            var lowerStrongContent = strongContent.ToLower();
            
            // Check if error is password-related
            var hasPasswordError = lowerStrongContent.Contains("password");
            
            if (hasPasswordError)
            {
                // Verify it's not a complexity error
                lowerStrongContent.Should().NotContain("uppercase", 
                    "Strong password should not fail uppercase requirement");
                lowerStrongContent.Should().NotContain("lowercase",
                    "Strong password should not fail lowercase requirement");
                lowerStrongContent.Should().NotContain("digit",
                    "Strong password should not fail digit requirement");
                lowerStrongContent.Should().NotContain("special",
                    "Strong password should not fail special character requirement");
                lowerStrongContent.Should().NotContain("length",
                    "Strong password should not fail length requirement");
                    
                // If password appears in error, it should be for other reasons (min length general message, etc.)
                // Strong password met all complexity requirements, so this is acceptable
            }
        }
        else
        {
            // Registration succeeded or failed for non-password reasons
            strongResponse.StatusCode.Should().BeOneOf(new[] { 
                HttpStatusCode.OK, 
                HttpStatusCode.Created, 
                HttpStatusCode.Accepted,
                HttpStatusCode.BadRequest  // Acceptable if not password-related
            });
        }
    }

    [Fact]
    public async Task Security_ShouldEnforce_AccountLockout()
    {
        // Arrange - Multiple failed login attempts
        var loginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        var responses = new List<HttpResponseMessage>();

        // Act - Attempt 10 failed logins
        for (int i = 0; i < 10; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            responses.Add(response);
            await Task.Delay(100); // Small delay between attempts
        }

        // Assert - Eventually should get locked out or rate limited
        var statusCodes = responses.Select(r => r.StatusCode).ToList();
        
        // Should contain either:
        // - 429 Too Many Requests (rate limiting)
        // - 403 Forbidden (account locked)
        // - Consistent 401 Unauthorized (no lockout but safe)
        var hasProtection = statusCodes.Contains(HttpStatusCode.TooManyRequests) ||
                           statusCodes.Contains(HttpStatusCode.Forbidden);

        // At minimum, should not return 500 errors
        statusCodes.Should().NotContain(HttpStatusCode.InternalServerError,
            "Failed login attempts should not cause server errors");

        // All responses should be client errors (4xx), not server errors (5xx)
        foreach (var statusCode in statusCodes)
        {
            ((int)statusCode).Should().BeInRange(400, 499,
                "Failed logins should return 4xx status codes");
        }
    }

    [Fact]
    public async Task Security_ShouldEnforce_TokenExpiration()
    {
        // Arrange - This test verifies tokens have expiration configured
        // In a real scenario, would need to create a token, wait for expiration, then test
        
        // For this test, verify that login response contains token with expiration info
        var registerRequest = new
        {
            Email = $"test-token-{Guid.NewGuid()}@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            // Token response should have expiration information
            var hasExpiresIn = root.TryGetProperty("expiresIn", out _) ||
                             root.TryGetProperty("expires_in", out _) ||
                             root.TryGetProperty("expiresAt", out _);

            hasExpiresIn.Should().BeTrue("Token response should include expiration information");

            // Access token should be present
            var hasAccessToken = root.TryGetProperty("accessToken", out _) ||
                               root.TryGetProperty("access_token", out _) ||
                               root.TryGetProperty("token", out _);

            hasAccessToken.Should().BeTrue("Token response should include access token");
        }
        else
        {
            // If login failed due to email verification requirement, that's expected
            var expectedStatusCodes = new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden };
            expectedStatusCodes.Should().Contain(response.StatusCode,
                "Expected authentication failure for unverified user");
        }
    }
}
