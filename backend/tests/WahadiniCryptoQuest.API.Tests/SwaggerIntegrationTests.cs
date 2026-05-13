using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests;

/// <summary>
/// Integration tests for Swagger/OpenAPI documentation
/// Verifies that Swagger UI and JSON endpoints are accessible and properly configured
/// </summary>
public class SwaggerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SwaggerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerUI_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/index.html");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task SwaggerJson_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task SwaggerJson_ContainsApiTitle()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        // Assert
        var title = jsonDoc.RootElement.GetProperty("info").GetProperty("title").GetString();
        Assert.Equal("WahadiniCryptoQuest API", title);
    }

    [Fact]
    public async Task SwaggerJson_ContainsApiVersion()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        // Assert
        var version = jsonDoc.RootElement.GetProperty("info").GetProperty("version").GetString();
        Assert.Equal("v1", version);
    }

    [Fact]
    public async Task SwaggerJson_ContainsBearerSecurityScheme()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        // Assert
        var securitySchemes = jsonDoc.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes");
        
        Assert.True(securitySchemes.TryGetProperty("Bearer", out var bearerScheme));
        Assert.Equal("http", bearerScheme.GetProperty("type").GetString());
        Assert.Equal("bearer", bearerScheme.GetProperty("scheme").GetString());
        Assert.Equal("JWT", bearerScheme.GetProperty("bearerFormat").GetString());
    }

    [Fact]
    public async Task SwaggerJson_ContainsAuthEndpoints()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        // Assert
        var paths = jsonDoc.RootElement.GetProperty("paths");
        
        // Verify key auth endpoints exist
        Assert.True(paths.TryGetProperty("/api/Auth/register", out _));
        Assert.True(paths.TryGetProperty("/api/Auth/login", out _));
        Assert.True(paths.TryGetProperty("/api/Auth/confirm-email", out _));
        Assert.True(paths.TryGetProperty("/api/Auth/refresh", out _));
    }

    [Fact]
    public async Task SwaggerJson_ContainsOpenApiVersion()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        // Assert
        var openApiVersion = jsonDoc.RootElement.GetProperty("openapi").GetString();
        Assert.StartsWith("3.0", openApiVersion); // OpenAPI 3.0.x
    }

    [Fact]
    public async Task SwaggerJson_ContainsApiDescription()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        // Assert
        var description = jsonDoc.RootElement.GetProperty("info").GetProperty("description").GetString();
        Assert.Contains("cryptocurrency education platform", description?.ToLower());
    }
}
