using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WahadiniCryptoQuest.API;
using WahadiniCryptoQuest.Core.DTOs.Progress;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for ProgressController
/// Tests HTTP endpoints for video progress tracking with authentication and rate limiting
/// </summary>
[Collection("Sequential")]
public class ProgressControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProgressControllerTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    #region Helper Methods

    private async Task<(User user, string token)> CreateAuthenticatedUserAsync(string email = "progresstest@test.com")
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = User.Create(email, "hashedpassword", "Progress", "TestUser");
            user.ConfirmEmail();
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user.Id, user.Email);
        return (user, token);
    }

    private string GenerateJwtToken(Guid userId, string email)
    {
        var secretKey = "ThisIsAVerySecureTestingSecretKeyThatIsAtLeast256BitsLong!123456789012345678901234567890";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", userId.ToString())
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

    private async Task<Core.Entities.Lesson> CreateTestLessonAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create category
        var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == "Progress Test Category");
        if (category == null)
        {
            category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Progress Test Category",
                Description = "Test Category",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Categories.Add(category);
        }

        // Create course
        var course = new Core.Entities.Course
        {
            Id = Guid.NewGuid(),
            Title = "Progress Test Course",
            Description = "Test Course",
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Courses.Add(course);

        // Create lesson
        var lesson = new Core.Entities.Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = "Progress Test Lesson",
            Description = "Test Lesson",
            YouTubeVideoId = "testVideoId",
            Duration = 10, // 10 minutes
            VideoDuration = 600, // 600 seconds
            RewardPoints = 50,
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Lessons.Add(lesson);

        await dbContext.SaveChangesAsync();
        return lesson;
    }

    #endregion

    #region GET Tests

    [Fact]
    public async Task GetProgress_Returns200WithProgressDto_WhenProgressExists()
    {
        // Arrange
        var (user, token) = await CreateAuthenticatedUserAsync("progress-get-exists@test.com");
        var lesson = await CreateTestLessonAsync();

        // Create existing progress
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var progress = UserProgress.Create(user.Id, lesson.Id);
            progress.UpdatePosition(300, 600); // 50% progress
            dbContext.UserProgress.Add(progress);
            await dbContext.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/lessons/{lesson.Id}/progress");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var progressDto = await response.Content.ReadFromJsonAsync<ProgressDto>(_jsonOptions);
        progressDto.Should().NotBeNull();
        progressDto!.LessonId.Should().Be(lesson.Id);
        progressDto.LastWatchedPosition.Should().Be(300);
        progressDto.CompletionPercentage.Should().Be(50m);
    }

    [Fact]
    public async Task GetProgress_Returns200WithNull_WhenNoProgressExists()
    {
        // Arrange
        var (user, token) = await CreateAuthenticatedUserAsync("progress-get-null@test.com");
        var lesson = await CreateTestLessonAsync();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/lessons/{lesson.Id}/progress");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetProgress_Returns401_WhenNotAuthenticated()
    {
        // Arrange
        var lesson = await CreateTestLessonAsync();

        // Act
        var response = await _client.GetAsync($"/api/lessons/{lesson.Id}/progress");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task UpdateProgress_Returns200WithResult_OnSuccess()
    {
        // Arrange
        var (user, token) = await CreateAuthenticatedUserAsync("progress-put-success@test.com");
        var lesson = await CreateTestLessonAsync();

        var updateDto = new UpdateProgressDto { WatchPosition = 200 };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/lessons/{lesson.Id}/progress", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UpdateProgressResultDto>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.CompletionPercentage.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateProgress_Returns400_WhenValidationFails()
    {
        // Arrange
        var (user, token) = await CreateAuthenticatedUserAsync("progress-put-invalid@test.com");
        var lesson = await CreateTestLessonAsync();

        var updateDto = new UpdateProgressDto { WatchPosition = -10 }; // Invalid negative position

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/lessons/{lesson.Id}/progress", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProgress_Returns404_WhenLessonNotFound()
    {
        // Arrange
        var (user, token) = await CreateAuthenticatedUserAsync("progress-put-notfound@test.com");
        var nonExistentLessonId = Guid.NewGuid();

        var updateDto = new UpdateProgressDto { WatchPosition = 100 };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/lessons/{nonExistentLessonId}/progress", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProgress_Returns401_WhenNotAuthenticated()
    {
        // Arrange
        var lesson = await CreateTestLessonAsync();
        var updateDto = new UpdateProgressDto { WatchPosition = 100 };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/lessons/{lesson.Id}/progress", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProgress_MarksComplete_When80PercentReached()
    {
        // Arrange
        var (user, token) = await CreateAuthenticatedUserAsync("progress-put-complete@test.com");
        var lesson = await CreateTestLessonAsync();

        var updateDto = new UpdateProgressDto { WatchPosition = 480 }; // 80% of 600 seconds

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/lessons/{lesson.Id}/progress", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UpdateProgressResultDto>(_jsonOptions);
        result.Should().NotBeNull();
        result!.IsNewlyCompleted.Should().BeTrue();
        result.PointsAwarded.Should().Be(50);

        // Verify completion in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var progress = await dbContext.UserProgress.FirstOrDefaultAsync(p => p.UserId == user.Id && p.LessonId == lesson.Id);
        progress.Should().NotBeNull();
        progress!.IsCompleted.Should().BeTrue();
        progress.CompletedAt.Should().NotBeNull();
        progress.RewardPointsClaimed.Should().BeTrue();
    }

    #endregion
}
