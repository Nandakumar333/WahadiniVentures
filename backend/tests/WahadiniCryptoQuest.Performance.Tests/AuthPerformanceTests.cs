using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace WahadiniCryptoQuest.Performance.Tests;

/// <summary>
/// Performance tests for authentication endpoints using NBomber
/// </summary>
public class AuthPerformanceTests : IClassFixture<PerformanceTestWebApplicationFactory>, IDisposable
{
    private readonly PerformanceTestWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _output;

    public AuthPerformanceTests(
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
    public void Register_ConstantLoad_ShouldHandleExpectedThroughput()
    {
        // Arrange
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost";
        var requestCounter = 0;

        var scenario = Scenario.Create("register_constant_load", async context =>
        {
            var uniqueId = Interlocked.Increment(ref requestCounter);
            var registerRequest = new
            {
                email = $"perftest{uniqueId}@example.com",
                username = $"perfuser{uniqueId}",
                password = "Test@1234",
                confirmPassword = "Test@1234",
                phoneNumber = $"+1234567{uniqueId:D4}"
            };

            var request = Http.CreateRequest("POST", $"{baseUrl}/api/auth/register")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(registerRequest),
                    Encoding.UTF8,
                    "application/json"));

            var response = await Http.Send(_httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 10, during: TimeSpan.FromSeconds(30))
        );

        // Act
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert
        var scenarioStats = stats.ScenarioStats[0];
        _output.WriteLine($"Scenario: {scenarioStats.ScenarioName}");
        _output.WriteLine($"Total Requests: {scenarioStats.Ok.Request.Count}");
        _output.WriteLine($"Failed Requests: {scenarioStats.Fail.Request.Count}");
        _output.WriteLine($"RPS: {scenarioStats.Ok.Request.RPS}");
        _output.WriteLine($"Min Latency: {scenarioStats.Ok.Latency.MinMs}ms");
        _output.WriteLine($"P50 Latency: {scenarioStats.Ok.Latency.Percent50}ms");
        _output.WriteLine($"P75 Latency: {scenarioStats.Ok.Latency.Percent75}ms");
        _output.WriteLine($"P95 Latency: {scenarioStats.Ok.Latency.Percent95}ms");
        _output.WriteLine($"P99 Latency: {scenarioStats.Ok.Latency.Percent99}ms");
        _output.WriteLine($"Max Latency: {scenarioStats.Ok.Latency.MaxMs}ms");

        // Performance assertions
        Assert.True(scenarioStats.Ok.Request.RPS > 5, 
            $"Expected RPS > 5, got {scenarioStats.Ok.Request.RPS}");
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 1000, 
            $"Expected P95 latency < 1000ms, got {scenarioStats.Ok.Latency.Percent95}ms");
    }

    [Fact(Skip = "Performance test - run manually")]
    public void Login_RampUp_ShouldHandleIncreasingLoad()
    {
        // Arrange - First create test users
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost";
        var testUsers = CreateTestUsers(50);

        var userIndex = 0;
        var scenario = Scenario.Create("login_ramp_up", async context =>
        {
            var user = testUsers[Interlocked.Increment(ref userIndex) % testUsers.Count];
            
            var loginRequest = new
            {
                emailOrUsername = user.Email,
                password = "Test@1234"
            };

            var request = Http.CreateRequest("POST", $"{baseUrl}/api/auth/login")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(loginRequest),
                    Encoding.UTF8,
                    "application/json"));

            var response = await Http.Send(_httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.RampingConstant(copies: 5, during: TimeSpan.FromSeconds(10)),
            Simulation.RampingConstant(copies: 10, during: TimeSpan.FromSeconds(10)),
            Simulation.RampingConstant(copies: 20, during: TimeSpan.FromSeconds(10))
        );

        // Act
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert
        var scenarioStats = stats.ScenarioStats[0];
        _output.WriteLine($"Scenario: {scenarioStats.ScenarioName}");
        _output.WriteLine($"Total Requests: {scenarioStats.Ok.Request.Count}");
        _output.WriteLine($"Failed Requests: {scenarioStats.Fail.Request.Count}");
        _output.WriteLine($"RPS: {scenarioStats.Ok.Request.RPS}");
        _output.WriteLine($"Mean Latency: {scenarioStats.Ok.Latency.MinMs}ms");
        _output.WriteLine($"P95 Latency: {scenarioStats.Ok.Latency.Percent95}ms");

        Assert.True(scenarioStats.Ok.Request.RPS > 3, 
            $"Expected RPS > 3, got {scenarioStats.Ok.Request.RPS}");
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 1500, 
            $"Expected P95 latency < 1500ms, got {scenarioStats.Ok.Latency.Percent95}ms");
    }

    [Fact(Skip = "Performance test - run manually")]
    public void Login_SpikeLoad_ShouldRecoverGracefully()
    {
        // Arrange
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost";
        var testUsers = CreateTestUsers(30);

        var userIndex = 0;
        var scenario = Scenario.Create("login_spike_test", async context =>
        {
            var user = testUsers[Interlocked.Increment(ref userIndex) % testUsers.Count];
            
            var loginRequest = new
            {
                emailOrUsername = user.Email,
                password = "Test@1234"
            };

            var request = Http.CreateRequest("POST", $"{baseUrl}/api/auth/login")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(loginRequest),
                    Encoding.UTF8,
                    "application/json"));

            var response = await Http.Send(_httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 5, during: TimeSpan.FromSeconds(10)),  // Normal load
            Simulation.KeepConstant(copies: 30, during: TimeSpan.FromSeconds(10)), // Spike
            Simulation.KeepConstant(copies: 5, during: TimeSpan.FromSeconds(10))   // Recovery
        );

        // Act
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert
        var scenarioStats = stats.ScenarioStats[0];
        _output.WriteLine($"Scenario: {scenarioStats.ScenarioName}");
        _output.WriteLine($"Total Requests: {scenarioStats.Ok.Request.Count}");
        _output.WriteLine($"Failed Requests: {scenarioStats.Fail.Request.Count}");
        _output.WriteLine($"RPS: {scenarioStats.Ok.Request.RPS}");
        _output.WriteLine($"Min Latency: {scenarioStats.Ok.Latency.MinMs}ms");
        _output.WriteLine($"P99 Latency: {scenarioStats.Ok.Latency.Percent99}ms");

        // Should handle spike with some tolerance for failures
        var successRate = (double)scenarioStats.Ok.Request.Count / 
            (scenarioStats.Ok.Request.Count + scenarioStats.Fail.Request.Count);
        
        Assert.True(successRate > 0.95, 
            $"Expected success rate > 95%, got {successRate * 100:F2}%");
    }

    [Fact(Skip = "Performance test - run manually")]
    public void TokenRefresh_ConstantLoad_ShouldHandleExpectedThroughput()
    {
        // Arrange
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost";
        var tokens = CreateTestUsersWithTokens(20);

        var tokenIndex = 0;
        var scenario = Scenario.Create("token_refresh_constant_load", async context =>
        {
            var token = tokens[Interlocked.Increment(ref tokenIndex) % tokens.Count];
            
            var refreshRequest = new
            {
                accessToken = token.AccessToken,
                refreshToken = token.RefreshToken
            };

            var request = Http.CreateRequest("POST", $"{baseUrl}/api/auth/refresh")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(refreshRequest),
                    Encoding.UTF8,
                    "application/json"));

            var response = await Http.Send(_httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 15, during: TimeSpan.FromSeconds(30))
        );

        // Act
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert
        var scenarioStats = stats.ScenarioStats[0];
        _output.WriteLine($"Scenario: {scenarioStats.ScenarioName}");
        _output.WriteLine($"Total Requests: {scenarioStats.Ok.Request.Count}");
        _output.WriteLine($"Failed Requests: {scenarioStats.Fail.Request.Count}");
        _output.WriteLine($"RPS: {scenarioStats.Ok.Request.RPS}");
        _output.WriteLine($"Min Latency: {scenarioStats.Ok.Latency.MinMs}ms");
        _output.WriteLine($"P95 Latency: {scenarioStats.Ok.Latency.Percent95}ms");

        Assert.True(scenarioStats.Ok.Request.RPS > 8,
            $"Expected RPS > 8, got {scenarioStats.Ok.Request.RPS}");
        Assert.True(scenarioStats.Ok.Latency.Percent95 < 800, 
            $"Expected P95 latency < 800ms, got {scenarioStats.Ok.Latency.Percent95}ms");
    }

    [Fact(Skip = "Performance test - run manually")]
    public void MixedWorkload_RealisticScenario_ShouldHandleMultipleEndpoints()
    {
        // Arrange
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost";
        var testUsers = CreateTestUsers(30);

        var registerCounter = 0;
        var userIndex = 0;

        // Scenario 1: Register new users (20% of traffic)
        var registerScenario = Scenario.Create("register_scenario", async context =>
        {
            var uniqueId = Interlocked.Increment(ref registerCounter);
            var registerRequest = new
            {
                email = $"mixed{uniqueId}@example.com",
                username = $"mixeduser{uniqueId}",
                password = "Test@1234",
                confirmPassword = "Test@1234",
                phoneNumber = $"+1987654{uniqueId:D4}"
            };

            var request = Http.CreateRequest("POST", $"{baseUrl}/api/auth/register")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(registerRequest),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 3, during: TimeSpan.FromSeconds(30))
        );

        // Scenario 2: Login existing users (50% of traffic)
        var loginScenario = Scenario.Create("login_scenario", async context =>
        {
            var user = testUsers[Interlocked.Increment(ref userIndex) % testUsers.Count];
            var loginRequest = new
            {
                emailOrUsername = user.Email,
                password = "Test@1234"
            };

            var request = Http.CreateRequest("POST", $"{baseUrl}/api/auth/login")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(
                    JsonSerializer.Serialize(loginRequest),
                    Encoding.UTF8,
                    "application/json"));

            return await Http.Send(_httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 8, during: TimeSpan.FromSeconds(30))
        );

        // Scenario 3: Health check (30% of traffic)
        var healthScenario = Scenario.Create("health_scenario", async context =>
        {
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/auth/health");
            return await Http.Send(_httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 5, during: TimeSpan.FromSeconds(30))
        );

        // Act
        var stats = NBomberRunner
            .RegisterScenarios(registerScenario, loginScenario, healthScenario)
            .Run();

        // Assert
        _output.WriteLine("=== Mixed Workload Performance Results ===");
        foreach (var scenarioStats in stats.ScenarioStats)
        {
            _output.WriteLine($"\nScenario: {scenarioStats.ScenarioName}");
            _output.WriteLine($"Total Requests: {scenarioStats.Ok.Request.Count}");
            _output.WriteLine($"Failed Requests: {scenarioStats.Fail.Request.Count}");
            _output.WriteLine($"RPS: {scenarioStats.Ok.Request.RPS}");
            _output.WriteLine($"Min Latency: {scenarioStats.Ok.Latency.MinMs}ms");
            _output.WriteLine($"P95 Latency: {scenarioStats.Ok.Latency.Percent95}ms");

            Assert.True(scenarioStats.Ok.Request.Count > 0,
                $"Scenario {scenarioStats.ScenarioName} should have successful requests");
        }
    }

    #region Helper Methods

    private List<TestUser> CreateTestUsers(int count)
    {
        var users = new List<TestUser>();
        
        for (int i = 0; i < count; i++)
        {
            var user = new TestUser
            {
                Email = $"loadtest{i}@example.com",
                Username = $"loaduser{i}",
                Password = "Test@1234"
            };

            // Register the user
            var registerRequest = new
            {
                email = user.Email,
                username = user.Username,
                password = user.Password,
                confirmPassword = user.Password,
                phoneNumber = $"+1555012{i:D4}"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json");

            var response = _httpClient.PostAsync("/api/auth/register", content).Result;
            
            if (response.IsSuccessStatusCode)
            {
                users.Add(user);
            }
        }

        return users;
    }

    private List<TokenPair> CreateTestUsersWithTokens(int count)
    {
        var tokens = new List<TokenPair>();
        
        for (int i = 0; i < count; i++)
        {
            // Register user
            var registerRequest = new
            {
                email = $"tokentest{i}@example.com",
                username = $"tokenuser{i}",
                password = "Test@1234",
                confirmPassword = "Test@1234",
                phoneNumber = $"+1555099{i:D4}"
            };

            var registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json");

            var registerResponse = _httpClient.PostAsync("/api/auth/register", registerContent).Result;

            if (!registerResponse.IsSuccessStatusCode)
                continue;

            // Login to get tokens
            var loginRequest = new
            {
                emailOrUsername = registerRequest.email,
                password = registerRequest.password
            };

            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            var loginResponse = _httpClient.PostAsync("/api/auth/login", loginContent).Result;

            if (loginResponse.IsSuccessStatusCode)
            {
                var responseContent = loginResponse.Content.ReadAsStringAsync().Result;
                var loginResult = JsonSerializer.Deserialize<LoginResponse>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (loginResult?.AccessToken != null && loginResult?.RefreshToken != null)
                {
                    tokens.Add(new TokenPair
                    {
                        AccessToken = loginResult.AccessToken,
                        RefreshToken = loginResult.RefreshToken
                    });
                }
            }
        }

        return tokens;
    }

    #endregion

    #region Helper Classes

    private class TestUser
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private class TokenPair
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    private class LoginResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    #endregion
}
