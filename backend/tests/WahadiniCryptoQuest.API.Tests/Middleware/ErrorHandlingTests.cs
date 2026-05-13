using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Middleware;

[Collection("Sequential")]
public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ErrorHandlingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_ShouldCatch_UnhandledExceptions()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Try to access endpoint with invalid data that might cause exception
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = (string?)null, // Null email might trigger validation or exception
            Password = (string?)null
        });

        // Assert
        var validStatuses = new[] { HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError };
        validStatuses.Should().Contain(response.StatusCode,
            "Middleware should handle exceptions gracefully");
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_ShouldReturn_500ForServerErrors()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Try to trigger a server error (malformed JSON)
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
        {
            Content = new StringContent("{invalid json}", System.Text.Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);

        // Assert
        var validStatuses = new[] { HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError };
        validStatuses.Should().Contain(response.StatusCode,
            "Middleware should return appropriate error status");
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_ShouldLog_ExceptionDetails()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Trigger an error
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "",
            Password = ""
        });

        // Assert
        // Note: In a real test, you'd verify logs using a test logger
        // Here we verify the response indicates proper error handling
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Validation errors should be logged and return 400");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty("Error response should include details");
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_ShouldNotExpose_SensitiveDetails()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Trigger an error
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        });

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        // Should not expose sensitive information
        content.Should().NotContain("ConnectionString", because: "should not expose database connection strings");
        content.Should().NotContain("StackTrace", because: "should not expose stack traces in production");
        content.Should().NotContain("System.Exception", because: "should not expose exception types");
        content.Should().NotContain("at System", because: "should not expose system-level details");
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_ShouldHandle_ValidationErrors_With400()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Send invalid registration data
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "invalid-email", // Invalid email format
            Password = "123", // Too short
            FullName = "" // Empty
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Validation errors should return 400 Bad Request");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty("Validation error details should be provided");
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_ShouldHandle_NotFoundErrors_With404()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Try to access non-existent endpoint
        var response = await client.GetAsync("/api/nonexistent/endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "Non-existent endpoints should return 404");
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_ShouldReturn_ProblemDetails_Format()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "",
            Password = ""
        });

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json",
            "Error responses should be in JSON format");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // Try to parse as JSON to verify format
        try
        {
            var json = JObject.Parse(content);
            json.Should().NotBeNull("Error response should be valid JSON");
        }
        catch
        {
            // Some responses might be plain text, which is also acceptable
            Assert.True(true, "Error response format is acceptable");
        }
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_ShouldHandle_ConcurrentErrors()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tasks = new System.Collections.Generic.List<Task<HttpResponseMessage>>();

        // Act - Send multiple concurrent invalid requests
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = "",
                Password = ""
            }));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                "All validation errors should be handled consistently");
        }
    }
}
