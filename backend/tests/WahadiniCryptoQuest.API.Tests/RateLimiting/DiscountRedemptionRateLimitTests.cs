using System.Net;
using System.Net.Http.Headers;
using Xunit;
using FluentAssertions;

namespace WahadiniCryptoQuest.API.Tests.RateLimiting;

public class DiscountRedemptionRateLimitTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;

    public DiscountRedemptionRateLimitTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RedeemDiscount_WithinRateLimit_AllRequestsSucceedOrFailForBusinessReasons()
    {
        // Arrange - Generate token for user
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, "user@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();
        var allowedRequests = 5; // Test with 5 requests (under the 10/minute limit)

        // Act - Make 5 redemption attempts
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < allowedRequests; i++)
        {
            var response = await _client.PostAsync($"/api/discounts/{discountId}/redeem", null);
            responses.Add(response);
        }

        // Assert - None should be rate limited (429)
        var rateLimitedResponses = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        rateLimitedResponses.Should().Be(0, "requests within rate limit should not be blocked");

        // Cleanup
        responses.ForEach(r => r.Dispose());
    }

    [Fact]
    public async Task RedeemDiscount_ExceedingRateLimit_ReturnsHttpTooManyRequests()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, "ratelimit@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();
        var excessiveRequests = 12; // Exceed 10 requests/minute limit

        // Act - Make 12 rapid redemption attempts
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < excessiveRequests; i++)
        {
            var response = await _client.PostAsync($"/api/discounts/{discountId}/redeem", null);
            responses.Add(response);

            // Small delay to simulate real-world conditions but still exceed rate
            await Task.Delay(50);
        }

        // Assert - At least some requests should be rate limited
        var rateLimitedResponses = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        rateLimitedResponses.Should().BeGreaterThan(0, "excessive requests should trigger rate limiting");

        // Verify first requests succeeded (before limit)
        responses.Take(10).Should().OnlyContain(r => r.StatusCode != HttpStatusCode.TooManyRequests,
            "first 10 requests should succeed before rate limit kicks in");

        // Cleanup
        responses.ForEach(r => r.Dispose());
    }

    [Fact]
    public async Task RedeemDiscount_DifferentUsers_EachHasOwnRateLimit()
    {
        // Arrange - Create two different users
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        var token1 = GenerateJwtToken(user1Id, "user1@test.com", new[] { "User" });
        var token2 = GenerateJwtToken(user2Id, "user2@test.com", new[] { "User" });

        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var discountId = Guid.NewGuid();
        var requestsPerUser = 6;

        // Act - Each user makes 6 requests
        var responses1 = new List<HttpResponseMessage>();
        var responses2 = new List<HttpResponseMessage>();

        for (int i = 0; i < requestsPerUser; i++)
        {
            responses1.Add(await client1.PostAsync($"/api/discounts/{discountId}/redeem", null));
            responses2.Add(await client2.PostAsync($"/api/discounts/{discountId}/redeem", null));
            await Task.Delay(50);
        }

        // Assert - Both users should be able to make requests without hitting rate limit
        var rateLimited1 = responses1.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var rateLimited2 = responses2.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        rateLimited1.Should().Be(0, "user1 should not be rate limited at 6 requests");
        rateLimited2.Should().Be(0, "user2 should not be rate limited at 6 requests");

        // Cleanup
        responses1.ForEach(r => r.Dispose());
        responses2.ForEach(r => r.Dispose());
        client1.Dispose();
        client2.Dispose();
    }

    [Fact]
    public async Task RedeemDiscount_RateLimitResponse_ContainsRetryAfterHeader()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, "retryafter@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();

        // Act - Exceed rate limit
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 12; i++)
        {
            responses.Add(await _client.PostAsync($"/api/discounts/{discountId}/redeem", null));
            await Task.Delay(50);
        }

        // Find first rate-limited response
        var rateLimitedResponse = responses.FirstOrDefault(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        // Assert
        if (rateLimitedResponse != null)
        {
            rateLimitedResponse.Headers.Should().ContainKey("Retry-After",
                "rate limited response should include Retry-After header");

            var retryAfterValue = rateLimitedResponse.Headers.GetValues("Retry-After").FirstOrDefault();
            retryAfterValue.Should().NotBeNullOrEmpty("Retry-After header should have a value");
        }

        // Cleanup
        responses.ForEach(r => r.Dispose());
    }

    [Fact]
    public async Task RedeemDiscount_OnlyRedemptionEndpoint_IsRateLimited()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, "getnotlimited@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Make many requests to GET endpoints
        var getDiscountsResponses = new List<HttpResponseMessage>();
        var getRedemptionsResponses = new List<HttpResponseMessage>();

        for (int i = 0; i < 15; i++)
        {
            getDiscountsResponses.Add(await _client.GetAsync("/api/discounts"));
            getRedemptionsResponses.Add(await _client.GetAsync("/api/discounts/my-redemptions"));
            await Task.Delay(50);
        }

        // Assert - GET endpoints should NOT be rate limited even with many requests
        var rateLimitedGetDiscounts = getDiscountsResponses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var rateLimitedGetRedemptions = getRedemptionsResponses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        rateLimitedGetDiscounts.Should().Be(0, "GET /api/discounts should not be rate limited");
        rateLimitedGetRedemptions.Should().Be(0, "GET /api/discounts/my-redemptions should not be rate limited");

        // Cleanup
        getDiscountsResponses.ForEach(r => r.Dispose());
        getRedemptionsResponses.ForEach(r => r.Dispose());
    }

    [Fact]
    public async Task RedeemDiscount_AfterWaitingPeriod_RateLimitResets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, "waitreset@test.com", new[] { "User" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var discountId = Guid.NewGuid();

        // Act - First burst: exceed rate limit
        var firstBurstResponses = new List<HttpResponseMessage>();
        for (int i = 0; i < 12; i++)
        {
            firstBurstResponses.Add(await _client.PostAsync($"/api/discounts/{discountId}/redeem", null));
            await Task.Delay(50);
        }

        var firstBurstRateLimited = firstBurstResponses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        firstBurstRateLimited.Should().BeGreaterThan(0, "first burst should trigger rate limiting");

        // Wait for rate limit window to reset (65 seconds to be safe for 60-second window)
        await Task.Delay(TimeSpan.FromSeconds(65));

        // Act - Second burst: should be allowed again
        var secondBurstResponses = new List<HttpResponseMessage>();
        for (int i = 0; i < 5; i++)
        {
            secondBurstResponses.Add(await _client.PostAsync($"/api/discounts/{discountId}/redeem", null));
            await Task.Delay(100);
        }

        // Assert - After waiting, requests should succeed again
        var secondBurstRateLimited = secondBurstResponses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        secondBurstRateLimited.Should().Be(0, "after waiting period, rate limit should reset");

        // Cleanup
        firstBurstResponses.ForEach(r => r.Dispose());
        secondBurstResponses.ForEach(r => r.Dispose());
    }

    // Helper method to generate JWT token
    private string GenerateJwtToken(Guid userId, string email, string[] roles)
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
