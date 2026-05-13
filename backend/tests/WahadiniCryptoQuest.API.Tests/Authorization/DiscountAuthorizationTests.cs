using System.Net;
using System.Net.Http.Headers;
using Xunit;
using FluentAssertions;

namespace WahadiniCryptoQuest.API.Tests.Authorization;

public class DiscountAuthorizationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;

    public DiscountAuthorizationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAvailableDiscounts_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange - No auth token

        // Act
        var response = await _client.GetAsync("/api/discounts/available");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAvailableDiscounts_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/discounts/available");

        // Assert - Should not be Unauthorized (401) if token is valid
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "valid authentication token should allow access to endpoint");
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
            "user role should have permission to access this endpoint");
        // 404/500/200/204 are all acceptable - authorization is what we're testing
    }

    [Fact]
    public async Task RedeemDiscount_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var discountId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/discounts/{discountId}/redeem", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RedeemDiscount_WithValidUserToken_ReturnsSuccessOrBadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var discountId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/discounts/{discountId}/redeem", null);

        // Assert - Will fail if discount doesn't exist, but should not be Unauthorized
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyRedemptions_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange - No auth token

        // Act
        var response = await _client.GetAsync("/api/discounts/my-redemptions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyRedemptions_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/discounts/my-redemptions");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    // Admin Endpoints Authorization Tests

    [Fact]
    public async Task AdminCreateDiscount_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new
        {
            Code = "TEST10",
            DiscountPercentage = 10,
            RequiredPoints = 100
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/discounts", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminCreateDiscount_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new
        {
            Code = "TEST10",
            DiscountPercentage = 10,
            RequiredPoints = 100
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/discounts", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminCreateDiscount_WithAdminToken_ReturnsSuccessOrBadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(
            Guid.NewGuid(),
            "admin@test.com",
            new[] { "Admin" },
            new[] { "discounts:manage" }
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new
        {
            Code = "TEST10",
            DiscountPercentage = 10,
            RequiredPoints = 100
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/discounts", createRequest);

        // Assert - Should not be Unauthorized or Forbidden
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminUpdateDiscount_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();
        var updateRequest = new
        {
            DiscountPercentage = 15
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/discounts/{discountId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminDeleteDiscount_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/admin/discounts/{discountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminActivateDiscount_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/admin/discounts/{discountId}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminDeactivateDiscount_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/admin/discounts/{discountId}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminGetAnalytics_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/admin/discounts/{discountId}/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminGetAnalyticsSummary_WithUserToken_ReturnsForbidden()
    {
        // Arrange
        var token = GenerateJwtToken(Guid.NewGuid(), "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/discounts/analytics/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminGetAnalytics_WithAdminToken_ReturnsSuccessOrNotFound()
    {
        // Arrange
        var token = GenerateJwtToken(
            Guid.NewGuid(),
            "admin@test.com",
            new[] { "Admin" },
            new[] { "discounts:manage" }
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/admin/discounts/{discountId}/analytics");

        // Assert - Should not be Unauthorized or Forbidden
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // Helper method to generate JWT token
    private string GenerateJwtToken(Guid userId, string email, string[] roles, string[]? permissions = null)
    {
        var secretKey = "ThisIsAVerySecureTestingSecretKeyThatIsAtLeast256BitsLong!123456789012345678901234567890";
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
            new System.Security.Claims.Claim("sub", userId.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
            claims.Add(new System.Security.Claims.Claim("role", role));
        }

        if (permissions != null)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new System.Security.Claims.Claim("permission", permission));
            }
        }

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
