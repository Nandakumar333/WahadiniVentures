using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using WahadiniCryptoQuest.API.Controllers;
using WahadiniCryptoQuest.DAL.Context;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

public class HealthControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<HealthController>> _mockLogger;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<HealthController>>();
        _controller = new HealthController(_context, _mockLogger.Object);
    }

    private class HealthResponse
    {
        public string status { get; set; } = string.Empty;
        public DateTime timestamp { get; set; }
        public string? service { get; set; }
        public string? database { get; set; }
        public string? reason { get; set; }
        public string? uptime { get; set; }
    }

    private T? DeserializeResponse<T>(object? value)
    {
        var json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
    }

    [Fact]
    public void GetHealth_ReturnsOkWithHealthyStatus()
    {
        // Act
        var result = _controller.GetHealth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var response = DeserializeResponse<HealthResponse>(okResult.Value);
        response!.status.Should().Be("Healthy");
        response.timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        response.service.Should().Be("WahadiniCryptoQuest API");
    }

    [Fact]
    public void GetHealth_DoesNotRequireAuthentication()
    {
        // Arrange - Controller has [AllowAnonymous] attribute
        var controllerType = typeof(HealthController);
        var attribute = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true);

        // Assert
        attribute.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetReadiness_WhenDatabaseConnected_ReturnsOkWithReadyStatus()
    {
        // Act
        var result = await _controller.GetReadiness(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var response = DeserializeResponse<HealthResponse>(okResult.Value);
        response!.status.Should().Be("Ready");
        response.database.Should().Be("Connected");
    }

    [Fact]
    public void GetLiveness_ReturnsOkWithAliveStatus()
    {
        // Act
        var result = _controller.GetLiveness();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var response = DeserializeResponse<HealthResponse>(okResult.Value);
        response!.status.Should().Be("Alive");
        response.timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        response.uptime.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetLiveness_ReturnsValidUptime()
    {
        // Act
        var result = _controller.GetLiveness() as OkObjectResult;
        var response = DeserializeResponse<HealthResponse>(result!.Value);

        // Assert - Uptime should be in format "d.hh:mm:ss"
        response!.uptime.Should().MatchRegex(@"^\d+\.\d{2}:\d{2}:\d{2}$");
    }

    [Fact]
    public async Task GetReadiness_CompletesQuickly()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        // Act
        await _controller.GetReadiness(CancellationToken.None);
        var duration = DateTime.UtcNow - startTime;

        // Assert - Should complete in less than 5 seconds
        duration.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetHealth_IncludesTimestamp()
    {
        // Act
        var result = _controller.GetHealth() as OkObjectResult;
        var response = DeserializeResponse<HealthResponse>(result!.Value);

        // Assert
        response!.timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
