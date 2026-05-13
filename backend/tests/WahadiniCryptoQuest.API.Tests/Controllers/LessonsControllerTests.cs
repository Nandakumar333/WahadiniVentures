using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WahadiniCryptoQuest.API;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for LessonsController
/// Tests HTTP endpoints, authorization, validation, reordering, and complete request/response flows
/// </summary>
[Collection("Sequential")]
public class LessonsControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public LessonsControllerTests(TestWebApplicationFactory<Program> factory)
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

    private async Task<string> GetAdminTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "lessonadmin@test.com");
        if (adminUser == null)
        {
            var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = Role.Create("Admin", "Administrator");
                dbContext.Roles.Add(adminRole);
                await dbContext.SaveChangesAsync();
            }

            adminUser = User.Create("lessonadmin@test.com", "hashedpassword", "Lesson", "Admin");
            adminUser.ConfirmEmail();
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();

            var userRole = Core.Entities.UserRole.Create(adminUser, adminRole);
            dbContext.UserRoles.Add(userRole);
            await dbContext.SaveChangesAsync();
        }

        return GenerateJwtToken(adminUser.Id, adminUser.Email, new[] { "Admin" });
    }

    private async Task<string> GetUserTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var regularUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "lessonuser@test.com");
        if (regularUser == null)
        {
            regularUser = User.Create("lessonuser@test.com", "hashedpassword", "Lesson", "User");
            regularUser.ConfirmEmail();
            dbContext.Users.Add(regularUser);
            await dbContext.SaveChangesAsync();
        }

        return GenerateJwtToken(regularUser.Id, regularUser.Email, new[] { "User" });
    }

    private string GenerateJwtToken(Guid userId, string email, string[] roles)
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

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
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

    private async Task<(Course course, Category category)> CreateTestCourseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = await dbContext.Categories.FirstOrDefaultAsync();
        if (category == null)
        {
            category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Lesson Test Category",
                Description = "Category for lesson tests",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
        }

        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "lessonadmin@test.com");
        if (adminUser == null)
        {
            adminUser = User.Create("lessonadmin@test.com", "hashedpassword", "Lesson", "Admin");
            adminUser.ConfirmEmail();
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
        }

        var course = new Course
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Title = "Lesson Test Course",
            Description = "Course for lesson tests",
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            RewardPoints = 10,
            EstimatedDuration = 60,
            CreatedByUserId = adminUser.Id,
            IsPublished = true,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync();

        return (course, category);
    }

    private async Task<Lesson> CreateTestLessonAsync(Guid courseId, int orderIndex = 1, bool isPremium = false)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = $"Test Lesson {orderIndex}",
            Description = $"Test lesson description {orderIndex}",
            YouTubeVideoId = "dQw4w9WgXcQ",
            OrderIndex = orderIndex,
            Duration = 10,
            IsPremium = isPremium,
            RewardPoints = 5,
            ContentMarkdown = "# Test content",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        dbContext.Lessons.Add(lesson);
        await dbContext.SaveChangesAsync();

        return lesson;
    }

    #endregion

    #region GET /api/lessons/{id} - Get Lesson by ID

    [Fact]
    public async Task GetLesson_WithValidId_ReturnsLessonDetails()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id);

        // Act
        var response = await _client.GetAsync($"/api/lessons/{lesson.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("id").GetGuid().Should().Be(lesson.Id);
        result.GetProperty("title").GetString().Should().Contain("Test Lesson");
    }

    [Fact]
    public async Task GetLesson_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/lessons/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        result.GetProperty("error").GetString().Should().Contain("not found");
    }

    [Fact]
    public async Task GetLesson_AllowsAnonymousAccess()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id);

        // Act - No token
        var response = await _client.GetAsync($"/api/lessons/{lesson.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLesson_WithIncludeTasksTrue_ReturnsTasks()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id);

        // Add tasks to the lesson
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var task1 = new LearningTask
            {
                Id = Guid.NewGuid(),
                LessonId = lesson.Id,
                Title = "Test Quiz Task",
                Description = "Complete this quiz",
                TaskType = TaskType.Quiz,
                TaskData = "{}",
                RewardPoints = 10,
                OrderIndex = 1,
                IsRequired = true,
                CreatedAt = DateTime.UtcNow
            };
            var task2 = new LearningTask
            {
                Id = Guid.NewGuid(),
                LessonId = lesson.Id,
                Title = "Test Screenshot Task",
                Description = "Submit screenshot",
                TaskType = TaskType.Screenshot,
                TaskData = "{}",
                RewardPoints = 20,
                OrderIndex = 2,
                IsRequired = false,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Tasks.AddRange(task1, task2);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/api/lessons/{lesson.Id}?includeTasks=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("id").GetGuid().Should().Be(lesson.Id);
        result.TryGetProperty("tasks", out var tasksElement).Should().BeTrue();

        if (tasksElement.ValueKind == JsonValueKind.Array)
        {
            var tasksArray = tasksElement.EnumerateArray().ToList();
            tasksArray.Should().HaveCount(2);
            tasksArray.First().GetProperty("title").GetString().Should().Be("Test Quiz Task");
            tasksArray.Last().GetProperty("title").GetString().Should().Be("Test Screenshot Task");
        }
    }

    [Fact]
    public async Task GetLesson_WithIncludeTasksFalse_DoesNotReturnTasks()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id);

        // Add tasks to the lesson
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var task1 = new LearningTask
            {
                Id = Guid.NewGuid(),
                LessonId = lesson.Id,
                Title = "Test Task",
                Description = "Test Description",
                TaskType = TaskType.Quiz,
                TaskData = "{}",
                RewardPoints = 10,
                OrderIndex = 1,
                IsRequired = true,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Tasks.Add(task1);
            await dbContext.SaveChangesAsync();
        }

        // Act - Explicitly set includeTasks=false
        var response = await _client.GetAsync($"/api/lessons/{lesson.Id}?includeTasks=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("id").GetGuid().Should().Be(lesson.Id);

        // Tasks should be null or empty
        if (result.TryGetProperty("tasks", out var tasksElement))
        {
            if (tasksElement.ValueKind == JsonValueKind.Array)
            {
                tasksElement.EnumerateArray().Should().BeEmpty();
            }
            else
            {
                tasksElement.ValueKind.Should().Be(JsonValueKind.Null);
            }
        }
    }

    [Fact]
    public async Task GetLesson_WithoutIncludeTasksParameter_UsesDefaultBehavior()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id);

        // Act - No includeTasks parameter (should default to false)
        var response = await _client.GetAsync($"/api/lessons/{lesson.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("id").GetGuid().Should().Be(lesson.Id);

        // Default behavior should not include tasks (or include as null/empty)
        if (result.TryGetProperty("tasks", out var tasksElement))
        {
            if (tasksElement.ValueKind == JsonValueKind.Array)
            {
                // If present as array, should be empty by default
                tasksElement.EnumerateArray().Should().BeEmpty();
            }
            else
            {
                // Or should be null
                tasksElement.ValueKind.Should().Be(JsonValueKind.Null);
            }
        }
    }

    #endregion

    #region GET /api/lessons/course/{courseId} - Get Lessons by Course

    [Fact]
    public async Task GetLessonsByCourse_WithValidCourseId_ReturnsLessonsList()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();
        await CreateTestLessonAsync(course.Id, 1);
        await CreateTestLessonAsync(course.Id, 2);
        await CreateTestLessonAsync(course.Id, 3);

        // Act
        var response = await _client.GetAsync($"/api/lessons/course/{course.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var lessons = JsonSerializer.Deserialize<List<JsonElement>>(content, _jsonOptions);

        lessons.Should().NotBeNull();
        lessons!.Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task GetLessonsByCourse_WithNonExistentCourse_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync($"/api/lessons/course/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var lessons = JsonSerializer.Deserialize<List<JsonElement>>(content, _jsonOptions);

        lessons.Should().NotBeNull();
        lessons!.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetLessonsByCourse_AllowsAnonymousAccess()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();

        // Act - No token
        var response = await _client.GetAsync($"/api/lessons/course/{course.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region POST /api/lessons/course/{courseId} - Create Lesson (Admin)

    [Fact]
    public async Task CreateLesson_WithValidAdminToken_CreatesSuccessfully()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var (course, _) = await CreateTestCourseAsync();

        var createDto = new CreateLessonDto
        {
            Title = "New Test Lesson",
            Description = "New test lesson description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 15,
            OrderIndex = 1,
            IsPremium = false,
            RewardPoints = 5,
            ContentMarkdown = "# New lesson content"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/lessons/course/{course.Id}", createDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("title").GetString().Should().Be(createDto.Title);
        result.GetProperty("description").GetString().Should().Be(createDto.Description);

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateLesson_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();

        var createDto = new CreateLessonDto
        {
            Title = "New Test Lesson",
            Description = "New test lesson description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 15,
            OrderIndex = 1,
            IsPremium = false,
            RewardPoints = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/lessons/course/{course.Id}", createDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateLesson_WithNonAdminToken_ReturnsForbidden()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var (course, _) = await CreateTestCourseAsync();

        var createDto = new CreateLessonDto
        {
            Title = "New Test Lesson",
            Description = "New test lesson description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 15,
            OrderIndex = 1,
            IsPremium = false,
            RewardPoints = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/lessons/course/{course.Id}", createDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateLesson_WithInvalidYouTubeId_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var (course, _) = await CreateTestCourseAsync();

        var createDto = new CreateLessonDto
        {
            Title = "New Test Lesson",
            Description = "New test lesson description",
            YouTubeVideoId = "invalid", // Invalid - must be 11 characters
            Duration = 15,
            OrderIndex = 1,
            IsPremium = false,
            RewardPoints = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/lessons/course/{course.Id}", createDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/lessons/{id} - Update Lesson (Admin)

    [Fact]
    public async Task UpdateLesson_WithValidAdminToken_UpdatesSuccessfully()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id);

        var updateDto = new UpdateLessonDto
        {
            Id = lesson.Id,
            CourseId = course.Id,
            Title = "Updated Lesson Title",
            Description = "Updated lesson description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 20,
            OrderIndex = 2,
            IsPremium = true,
            RewardPoints = 10,
            ContentMarkdown = "# Updated content"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/lessons/{lesson.Id}", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("title").GetString().Should().Be(updateDto.Title);
        result.GetProperty("description").GetString().Should().Be(updateDto.Description);
    }

    [Fact]
    public async Task UpdateLesson_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var invalidLessonId = Guid.NewGuid();
        var (course, _) = await CreateTestCourseAsync();

        var updateDto = new UpdateLessonDto
        {
            Id = invalidLessonId,
            CourseId = course.Id,
            Title = "Updated Lesson Title",
            Description = "Updated lesson description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 20,
            OrderIndex = 2,
            IsPremium = false,
            RewardPoints = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/lessons/{invalidLessonId}", updateDto, _jsonOptions);

        // Assert - Service returns NotFound when lesson doesn't exist
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateLesson_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id);

        var updateDto = new UpdateLessonDto
        {
            Title = "Updated Lesson Title",
            Description = "Updated lesson description",
            YouTubeVideoId = "anotherVideoId",
            Duration = 20,
            OrderIndex = 2,
            IsPremium = false,
            RewardPoints = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/lessons/{lesson.Id}", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region DELETE /api/lessons/{id} - Delete Lesson (Admin)

    [Fact]
    public async Task DeleteLesson_WithValidAdminToken_DeletesSuccessfully()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id);

        // Act
        var response = await _client.DeleteAsync($"/api/lessons/{lesson.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteLesson_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync($"/api/lessons/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteLesson_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id);

        // Act
        var response = await _client.DeleteAsync($"/api/lessons/{lesson.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region PUT /api/lessons/reorder - Reorder Lessons (Admin)

    [Fact]
    public async Task ReorderLessons_WithValidAdminToken_ReordersSuccessfully()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var (course, _) = await CreateTestCourseAsync();
        var lesson1 = await CreateTestLessonAsync(course.Id, 1);
        var lesson2 = await CreateTestLessonAsync(course.Id, 2);
        var lesson3 = await CreateTestLessonAsync(course.Id, 3);

        var reorderDto = new ReorderLessonsDto
        {
            CourseId = course.Id,
            LessonOrders = new[]
            {
                new LessonOrderDto { LessonId = lesson3.Id, OrderIndex = 1 },
                new LessonOrderDto { LessonId = lesson1.Id, OrderIndex = 2 },
                new LessonOrderDto { LessonId = lesson2.Id, OrderIndex = 3 }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/lessons/reorder", reorderDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("message").GetString().Should().Contain("reordered successfully");
    }

    [Fact]
    public async Task ReorderLessons_WithInvalidLessonId_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var (course, _) = await CreateTestCourseAsync();
        var lesson1 = await CreateTestLessonAsync(course.Id, 1);

        var reorderDto = new ReorderLessonsDto
        {
            CourseId = course.Id,
            LessonOrders = new[]
            {
                new LessonOrderDto { LessonId = lesson1.Id, OrderIndex = 1 },
                new LessonOrderDto { LessonId = Guid.NewGuid(), OrderIndex = 2 } // Invalid ID
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/lessons/reorder", reorderDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReorderLessons_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var (course, _) = await CreateTestCourseAsync();

        var reorderDto = new ReorderLessonsDto
        {
            CourseId = course.Id,
            LessonOrders = new[]
            {
                new LessonOrderDto { LessonId = Guid.NewGuid(), OrderIndex = 1 }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/lessons/reorder", reorderDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReorderLessons_WithNonAdminToken_ReturnsForbidden()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var (course, _) = await CreateTestCourseAsync();
        var lesson = await CreateTestLessonAsync(course.Id, 1);

        var reorderDto = new ReorderLessonsDto
        {
            CourseId = course.Id,
            LessonOrders = new[]
            {
                new LessonOrderDto { LessonId = lesson.Id, OrderIndex = 1 }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/lessons/reorder", reorderDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReorderLessons_EmptyList_ReturnsSuccess()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var (course, _) = await CreateTestCourseAsync();

        var reorderDto = new ReorderLessonsDto
        {
            CourseId = course.Id,
            LessonOrders = Array.Empty<LessonOrderDto>()
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/lessons/reorder", reorderDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
