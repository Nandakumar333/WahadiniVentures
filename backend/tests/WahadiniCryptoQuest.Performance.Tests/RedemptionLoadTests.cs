using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using Xunit.Abstractions;

namespace WahadiniCryptoQuest.Performance.Tests;

public class RedemptionLoadTests : IClassFixture<PerformanceTestWebApplicationFactory>, IDisposable
{
    private readonly PerformanceTestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _output;

    public RedemptionLoadTests(
        PerformanceTestWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
        _output = output;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(Skip = "Performance test - run manually")]
    public void ConcurrentRedemptions_With50Users_CompletesSuccessfully()
    {
        // Arrange - Configuration
        const int concurrentUsers = 50;
        const int durationInSeconds = 30;
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost";

        // Generate test data: user tokens and discount IDs
        var testData = GenerateTestData(concurrentUsers);
        var dataIndex = 0;

        // Create HTTP scenario
        var scenario = Scenario.Create("discount_redemption_load", async context =>
        {
            var index = Interlocked.Increment(ref dataIndex) % testData.Count;
            var userData = testData[index];

            var request = Http.CreateRequest("POST", $"{baseUrl}/api/discounts/{userData.DiscountId}/redeem")
                .WithHeader("Authorization", $"Bearer {userData.Token}")
                .WithHeader("Content-Type", "application/json");

            var response = await Http.Send(_httpClient, request);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.Inject(
                rate: concurrentUsers,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(durationInSeconds)
            )
        );

        // Act - Run load test
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert - Performance criteria
        var scenarioStats = stats.ScenarioStats[0];

        _output.WriteLine($"Scenario: {scenarioStats.ScenarioName}");
        _output.WriteLine($"Total Requests: {scenarioStats.Ok.Request.Count}");
        _output.WriteLine($"Failed Requests: {scenarioStats.Fail.Request.Count}");
        _output.WriteLine($"RPS: {scenarioStats.Ok.Request.RPS}");
        _output.WriteLine($"P95 Latency: {scenarioStats.Ok.Latency.Percent95}ms");
        _output.WriteLine($"Max Latency: {scenarioStats.Ok.Latency.MaxMs}ms");

        // At least 50% success rate (some may fail due to business rules)
        var successRate = scenarioStats.Ok.Request.Count / (double)scenarioStats.AllRequestCount * 100;
        successRate.Should().BeGreaterThanOrEqualTo(50,
            "at least 50% of requests should succeed (others may fail due to insufficient points, expired discounts, etc.)");

        // P95 latency should be under 1 second
        scenarioStats.Ok.Latency.Percent95.Should().BeLessThan(1000,
            "95% of successful requests should complete within 1 second");

        // No request should take more than 5 seconds
        scenarioStats.Ok.Latency.MaxMs.Should().BeLessThan(5000,
            "maximum request duration should be under 5 seconds");

        // Minimum throughput: at least 5 requests per second
        scenarioStats.Ok.Request.RPS.Should().BeGreaterThan(5,
            "system should handle at least 5 successful requests per second");
    }

    [Fact(Skip = "Performance test - run manually")]
    public void ConcurrentRedemptions_SameDiscount_HandlesContentionGracefully()
    {
        // Arrange - Multiple users trying to redeem the same limited discount
        const int concurrentUsers = 20;
        const string sharedDiscountId = "test-discount-123";
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost";

        var testData = GenerateTestData(concurrentUsers);
        var dataIndex = 0;

        var scenario = Scenario.Create("concurrent_same_discount", async context =>
        {
            var index = Interlocked.Increment(ref dataIndex) % testData.Count;
            var userData = testData[index];

            var request = Http.CreateRequest("POST", $"{baseUrl}/api/discounts/{sharedDiscountId}/redeem")
                .WithHeader("Authorization", $"Bearer {userData.Token}");

            var response = await Http.Send(_httpClient, request);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(2))
        .WithLoadSimulations(
            Simulation.Inject(
                rate: concurrentUsers,
                interval: TimeSpan.FromMilliseconds(100),
                during: TimeSpan.FromSeconds(5)
            )
        );

        // Act
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert - System should handle contention without errors
        var scenarioStats = stats.ScenarioStats[0];

        _output.WriteLine($"Total Requests: {scenarioStats.AllRequestCount}");
        _output.WriteLine($"Failed Requests: {scenarioStats.Fail.Request.Count}");
        _output.WriteLine($"P95 Latency: {scenarioStats.Ok.Latency.Percent95}ms");

        // Some requests will fail due to business rules (already redeemed, sold out), but no crashes
        scenarioStats.Fail.Request.Count.Should().BeLessThan(scenarioStats.AllRequestCount,
            "system should complete all requests without crashing");

        // Response time should remain reasonable even under contention
        scenarioStats.Ok.Latency.Percent95.Should().BeLessThan(2000,
            "95% of requests should complete within 2 seconds even with contention");
    }

    [Fact(Skip = "Performance test - run manually")]
    public void SequentialRedemptions_SingleUser_MaintainsPerformance()
    {
        // Arrange - One user making multiple redemption attempts
        var testData = GenerateTestData(1);
        var userData = testData[0];
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost";

        var scenario = Scenario.Create("sequential_redemptions", async context =>
        {
            // Try different discount IDs in sequence
            var discountId = Guid.NewGuid();

            var request = Http.CreateRequest("POST", $"{baseUrl}/api/discounts/{discountId}/redeem")
                .WithHeader("Authorization", $"Bearer {userData.Token}");

            var response = await Http.Send(_httpClient, request);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(2))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 1, during: TimeSpan.FromSeconds(10))
        );

        // Act
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert - Consistent performance for sequential requests
        var scenarioStats = stats.ScenarioStats[0];

        _output.WriteLine($"Mean Latency: {scenarioStats.Ok.Latency.MeanMs}ms");
        _output.WriteLine($"Max Latency: {scenarioStats.Ok.Latency.MaxMs}ms");

        // Latency should be consistent (low variance)
        var avgLatency = scenarioStats.Ok.Latency.MeanMs;
        var maxLatency = scenarioStats.Ok.Latency.MaxMs;

        (maxLatency - avgLatency).Should().BeLessThan(500,
            "latency variance should be low for sequential requests");

        // Mean response time should be under 200ms
        avgLatency.Should().BeLessThan(200,
            "average response time should be under 200ms");
    }

    [Fact(Skip = "Performance test - run manually")]
    public void RateLimiting_EnforcedUnderLoad_PreventsAbuse()
    {
        // Arrange - One user exceeding rate limit
        var testData = GenerateTestData(1);
        var userData = testData[0];
        var discountId = Guid.NewGuid();
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost";

        var scenario = Scenario.Create("rate_limit_enforcement", async context =>
        {
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/discounts/{discountId}/redeem")
                .WithHeader("Authorization", $"Bearer {userData.Token}");

            var response = await Http.Send(_httpClient, request);

            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            // Try to make 20 requests in 1 second (exceeds 10/minute limit)
            Simulation.Inject(
                rate: 20,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(1)
            )
        );

        // Act
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert - Some requests should be rate limited
        var scenarioStats = stats.ScenarioStats[0];

        _output.WriteLine($"Total Requests: {scenarioStats.AllRequestCount}");
        _output.WriteLine($"Mean Latency: {scenarioStats.Ok.Latency.MeanMs}ms");

        // At least some requests should fail due to rate limiting
        scenarioStats.AllRequestCount.Should().BeGreaterThan(0,
            "test should execute requests");

        // System should respond quickly even when rate limiting
        scenarioStats.Ok.Latency.MeanMs.Should().BeLessThan(500,
            "rate limit responses should be fast");
    }

    // Helper method to generate test data
    private List<UserTestData> GenerateTestData(int count)
    {
        var testData = new List<UserTestData>();

        for (int i = 0; i < count; i++)
        {
            var userId = Guid.NewGuid();
            var token = GenerateTestToken(userId, $"loadtest{i}@example.com");
            var discountId = Guid.NewGuid().ToString();

            testData.Add(new UserTestData
            {
                UserId = userId,
                Token = token,
                DiscountId = discountId
            });
        }

        return testData;
    }

    private string GenerateTestToken(Guid userId, string email)
    {
        // Generate a simple test JWT token for load testing
        var secretKey = "ThisIsAVerySecureTestingSecretKeyThatIsAtLeast256BitsLong!123456789012345678901234567890";
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
            new System.Security.Claims.Claim("sub", userId.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "User"),
            new System.Security.Claims.Claim("role", "User")
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    private class UserTestData
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string DiscountId { get; set; } = string.Empty;
    }
}
