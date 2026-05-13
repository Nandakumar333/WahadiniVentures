using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for CoursesController
/// Tests HTTP endpoints, authorization, validation, and complete request/response flows
/// </summary>
[Collection("Sequential")]
public class CoursesControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public CoursesControllerTests(TestWebApplicationFactory<Program> factory)
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

        // Find or create admin user
        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
        if (adminUser == null)
        {
            var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = Role.Create("Admin", "Administrator");
                dbContext.Roles.Add(adminRole);
                await dbContext.SaveChangesAsync();
            }

            adminUser = User.Create("admin@test.com", "hashedpassword", "Admin", "User");
            adminUser.ConfirmEmail();
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();

            var userRole = Core.Entities.UserRole.Create(adminUser, adminRole);
            dbContext.UserRoles.Add(userRole);
            await dbContext.SaveChangesAsync();
        }

        // Admin permissions for testing
        var adminPermissions = new[]
        {
            "courses:create", "courses:edit", "courses:delete", "courses:manage",
            "courses:read", "courses:publish", "courses:view",
            "lessons:create", "lessons:edit", "lessons:delete",
            "tasks:create", "tasks:edit", "tasks:delete"
        };

        return GenerateJwtToken(adminUser.Id, adminUser.Email, new[] { "Admin" }, adminPermissions);
    }

    private async Task<string> GetUserTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var regularUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "user@test.com");
        if (regularUser == null)
        {
            regularUser = User.Create("user@test.com", "hashedpassword", "Regular", "User");
            regularUser.ConfirmEmail();
            dbContext.Users.Add(regularUser);
            await dbContext.SaveChangesAsync();
        }

        // Regular user permissions for testing
        var userPermissions = new[]
        {
            "courses:view", "courses:free",
            "lessons:view",
            "tasks:view", "tasks:submit",
            "profile:read", "profile:edit"
        };

        return GenerateJwtToken(regularUser.Id, regularUser.Email, new[] { "User" }, userPermissions);
    }

    private string GenerateJwtToken(Guid userId, string email, string[] roles, string[]? permissions = null)
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

        // Add permission claims if provided
        if (permissions != null)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
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

    private async Task<Category> CreateTestCategoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = await dbContext.Categories.FirstOrDefaultAsync();
        if (category == null)
        {
            category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test category for courses",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
        }

        return category;
    }

    private async Task<Course> CreateTestCourseAsync(bool isPublished = true, bool isPremium = false)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = await CreateTestCategoryAsync();
        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
        if (adminUser == null)
        {
            adminUser = User.Create("admin@test.com", "hashedpassword", "Admin", "User");
            adminUser.ConfirmEmail();
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
        }

        var course = new Course
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Title = "Test Course",
            Description = "Test course description",
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = isPremium,
            RewardPoints = 10,
            EstimatedDuration = 60,
            CreatedByUserId = adminUser.Id,
            IsPublished = false,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        dbContext.Courses.Add(course);

        if (isPublished)
        {
            // Add a lesson to allow publishing
            var lesson = new Lesson
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                Title = "Test Lesson",
                Description = "Test lesson description",
                YouTubeVideoId = "dQw4w9WgXcQ",
                OrderIndex = 1,
                Duration = 10,
                IsPremium = false,
                RewardPoints = 5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            dbContext.Lessons.Add(lesson);
            course.Publish();
        }

        await dbContext.SaveChangesAsync();

        return course;
    }

    #endregion

    #region GET /api/courses - Get Courses with Filters

    [Fact]
    public async Task GetCourses_WithoutFilters_ReturnsPublishedCourses()
    {
        // Arrange
        await CreateTestCourseAsync(isPublished: true);
        await CreateTestCourseAsync(isPublished: true);

        // Add authentication token for browsing courses
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("items").GetArrayLength().Should().BeGreaterOrEqualTo(2);
        result.GetProperty("totalCount").GetInt32().Should().BeGreaterOrEqualTo(2);
        result.GetProperty("pageNumber").GetInt32().Should().Be(1);
        result.GetProperty("pageSize").GetInt32().Should().Be(10);
    }

    [Fact]
    public async Task GetCourses_WithCategoryFilter_ReturnsFilteredCourses()
    {
        // Arrange
        var category = await CreateTestCategoryAsync();
        await CreateTestCourseAsync(isPublished: true);

        // Add authentication token
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/courses?categoryId={category.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("items").GetArrayLength().Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetCourses_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            await CreateTestCourseAsync(isPublished: true);
        }

        // Add authentication token
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Get page 2 with 10 items per page
        var response = await _client.GetAsync("/api/courses?page=2&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("pageNumber").GetInt32().Should().Be(2);
        result.GetProperty("pageSize").GetInt32().Should().Be(10);
        result.GetProperty("hasPreviousPage").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetCourses_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Create a fresh client to avoid response caching issues
        using var client = _factory.CreateClient();

        // Add authentication token
        var token = await GetUserTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - add cache-busting parameter
        var cacheBuster = Guid.NewGuid().ToString("N");
        var response = await client.GetAsync($"/api/courses?page=0&_={cacheBuster}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        result.GetProperty("error").GetString().Should().Contain("Page number must be greater than 0");
    }

    [Fact]
    public async Task GetCourses_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Create a fresh client to avoid response caching issues
        using var client = _factory.CreateClient();

        // Add authentication token
        var token = await GetUserTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - add cache-busting parameter
        var cacheBuster = Guid.NewGuid().ToString("N");
        var response = await client.GetAsync($"/api/courses?pageSize=100&_={cacheBuster}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        result.GetProperty("error").GetString().Should().Contain("Page size must be between 1 and 50");
    }

    [Fact]
    public async Task GetCourses_WithDifficultyFilter_ReturnsFilteredCourses()
    {
        // Arrange
        await CreateTestCourseAsync(isPublished: true);

        // Add authentication token
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/courses?difficultyLevel=0"); // Beginner

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("items").GetArrayLength().Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetCourses_WithPremiumFilter_ReturnsFilteredCourses()
    {
        // Arrange
        await CreateTestCourseAsync(isPublished: true, isPremium: true);

        // Add authentication token
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/courses?isPremium=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("items").GetArrayLength().Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetCourses_IncludesCacheHeaders()
    {
        // Add authentication token
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.CacheControl.Should().NotBeNull();
        response.Headers.CacheControl!.Public.Should().BeTrue();
        response.Headers.CacheControl.MaxAge.Should().Be(TimeSpan.FromMinutes(5));
    }

    #endregion

    #region GET /api/courses/admin - Get All Courses (Admin)

    [Fact]
    public async Task GetAdminCourses_WithValidAdminToken_ReturnsAllCourses()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await CreateTestCourseAsync(isPublished: true);
        await CreateTestCourseAsync(isPublished: false); // Unpublished

        // Act
        var response = await _client.GetAsync("/api/courses/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("items").GetArrayLength().Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetAdminCourses_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/courses/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAdminCourses_WithNonAdminToken_ReturnsForbidden()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/courses/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GET /api/courses/my-courses - Get User's Enrolled Courses

    [Fact]
    public async Task GetMyCourses_WithValidToken_ReturnsEnrolledCourses()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "user@test.com");
        var course = await CreateTestCourseAsync(isPublished: true);

        var enrollment = new UserCourseEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = user!.Id,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow,
            CompletionPercentage = 0,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.UserCourseEnrollments.Add(enrollment);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/courses/my-courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var courses = JsonSerializer.Deserialize<List<JsonElement>>(content, _jsonOptions);

        courses.Should().NotBeNull();
        courses!.Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetMyCourses_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/courses/my-courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/courses/{id} - Get Course Detail

    [Fact]
    public async Task GetCourseDetail_WithValidId_ReturnsCourseDetails()
    {
        // Arrange
        var course = await CreateTestCourseAsync(isPublished: true);

        // Act
        var response = await _client.GetAsync($"/api/courses/{course.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("id").GetGuid().Should().Be(course.Id);
        result.GetProperty("title").GetString().Should().Be(course.Title);
    }

    [Fact]
    public async Task GetCourseDetail_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/courses/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        result.GetProperty("error").GetString().Should().Contain("not found");
    }

    [Fact]
    public async Task GetCourseDetail_AllowsAnonymousAccess()
    {
        // Arrange
        var course = await CreateTestCourseAsync(isPublished: true);

        // Act - No token
        var response = await _client.GetAsync($"/api/courses/{course.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region POST /api/courses/{id}/enroll - Enroll in Course

    [Fact]
    public async Task EnrollInCourse_WithValidToken_EnrollsSuccessfully()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var course = await CreateTestCourseAsync(isPublished: true);

        // Act
        var response = await _client.PostAsync($"/api/courses/{course.Id}/enroll", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("message").GetString().Should().Contain("Successfully enrolled");
        result.GetProperty("courseId").GetGuid().Should().Be(course.Id);
    }

    [Fact]
    public async Task EnrollInCourse_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var course = await CreateTestCourseAsync(isPublished: true);

        // Act
        var response = await _client.PostAsync($"/api/courses/{course.Id}/enroll", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EnrollInCourse_AlreadyEnrolled_ReturnsConflict()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var course = await CreateTestCourseAsync(isPublished: true);

        // Enroll once
        await _client.PostAsync($"/api/courses/{course.Id}/enroll", null);

        // Act - Try to enroll again
        var response = await _client.PostAsync($"/api/courses/{course.Id}/enroll", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        result.GetProperty("error").GetString().Should().Contain("already enrolled");
    }

    [Fact]
    public async Task EnrollInCourse_FreeUserEnrollsInPremiumCourse_ReturnsForbidden()
    {
        // Arrange - T151: Premium access gate test
        var token = await GetUserTokenAsync(); // Regular free user
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var premiumCourse = await CreateTestCourseAsync(isPublished: true, isPremium: true);

        // Act - Free user tries to enroll in premium course
        var response = await _client.PostAsync($"/api/courses/{premiumCourse.Id}/enroll", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        result.GetProperty("error").GetString().Should().Contain("premium");
    }

    #endregion

    #region POST /api/courses - Create Course (Admin)

    [Fact]
    public async Task CreateCourse_WithValidAdminToken_CreatesSuccessfully()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var category = await CreateTestCategoryAsync();

        var createDto = new CreateCourseDto
        {
            Title = "New Test Course",
            Description = "New test course description",
            CategoryId = category.Id,
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            ThumbnailUrl = "https://example.com/image.jpg",
            RewardPoints = 10,
            EstimatedDuration = 60
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/courses", createDto, _jsonOptions);

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
    public async Task CreateCourse_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var category = await CreateTestCategoryAsync();

        var createDto = new CreateCourseDto
        {
            Title = "New Test Course",
            Description = "New test course description",
            CategoryId = category.Id,
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            ThumbnailUrl = "https://example.com/image.jpg",
            RewardPoints = 10,
            EstimatedDuration = 60
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/courses", createDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCourse_WithNonAdminToken_ReturnsForbidden()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var category = await CreateTestCategoryAsync();

        var createDto = new CreateCourseDto
        {
            Title = "New Test Course",
            Description = "New test course description",
            CategoryId = category.Id,
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            ThumbnailUrl = "https://example.com/image.jpg",
            RewardPoints = 10,
            EstimatedDuration = 60
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/courses", createDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region PUT /api/courses/{id} - Update Course (Admin)

    [Fact]
    public async Task UpdateCourse_WithValidAdminToken_UpdatesSuccessfully()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var course = await CreateTestCourseAsync(isPublished: true);
        var category = await CreateTestCategoryAsync();

        var updateDto = new UpdateCourseDto
        {
            Title = "Updated Course Title",
            Description = "Updated course description",
            CategoryId = category.Id,
            DifficultyLevel = DifficultyLevel.Intermediate,
            IsPremium = true,
            ThumbnailUrl = "https://example.com/updated.jpg",
            RewardPoints = 20,
            EstimatedDuration = 90
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/courses/{course.Id}", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("title").GetString().Should().Be(updateDto.Title);
        result.GetProperty("description").GetString().Should().Be(updateDto.Description);
    }

    [Fact]
    public async Task UpdateCourse_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var category = await CreateTestCategoryAsync();

        var updateDto = new UpdateCourseDto
        {
            Title = "Updated Course Title",
            Description = "Updated course description",
            CategoryId = category.Id,
            DifficultyLevel = DifficultyLevel.Intermediate,
            IsPremium = false,
            ThumbnailUrl = "https://example.com/updated.jpg",
            RewardPoints = 20,
            EstimatedDuration = 90
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/courses/{Guid.NewGuid()}", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCourse_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var course = await CreateTestCourseAsync(isPublished: true);
        var category = await CreateTestCategoryAsync();

        var updateDto = new UpdateCourseDto
        {
            Title = "Updated Course Title",
            Description = "Updated course description",
            CategoryId = category.Id,
            DifficultyLevel = DifficultyLevel.Intermediate,
            IsPremium = false,
            ThumbnailUrl = "https://example.com/updated.jpg",
            RewardPoints = 20,
            EstimatedDuration = 90
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/courses/{course.Id}", updateDto, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region DELETE /api/courses/{id} - Delete Course (Admin)

    [Fact]
    public async Task DeleteCourse_WithValidAdminToken_DeletesSuccessfully()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var course = await CreateTestCourseAsync(isPublished: true);

        // Act
        var response = await _client.DeleteAsync($"/api/courses/{course.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCourse_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync($"/api/courses/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCourse_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var course = await CreateTestCourseAsync(isPublished: true);

        // Act
        var response = await _client.DeleteAsync($"/api/courses/{course.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region PUT /api/courses/{id}/publish - Publish Course (Admin)

    [Fact]
    public async Task PublishCourse_WithValidAdminToken_PublishesSuccessfully()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create unpublished course with lessons
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = await CreateTestCategoryAsync();
        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");

        var course = new Course
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Title = "Course to Publish",
            Description = "Course description",
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            RewardPoints = 10,
            EstimatedDuration = 60,
            CreatedByUserId = adminUser!.Id,
            IsPublished = false,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync();

        // Add a lesson (required for publishing)
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = "Test Lesson",
            Description = "Test lesson description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            OrderIndex = 1,
            Duration = 10,
            IsPremium = false,
            RewardPoints = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        dbContext.Lessons.Add(lesson);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.PutAsync($"/api/courses/{course.Id}/publish", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("message").GetString().Should().Contain("published successfully");
    }

    [Fact]
    public async Task PublishCourse_WithoutLessons_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create unpublished course without lessons
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = await CreateTestCategoryAsync();
        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");

        var course = new Course
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Title = "Course without Lessons",
            Description = "Course description",
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            RewardPoints = 10,
            EstimatedDuration = 60,
            CreatedByUserId = adminUser!.Id,
            IsPublished = false,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.PutAsync($"/api/courses/{course.Id}/publish", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        result.GetProperty("error").GetString().Should().Contain("lesson");
    }

    [Fact]
    public async Task PublishCourse_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsync($"/api/courses/{Guid.NewGuid()}/publish", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PublishCourse_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var course = await CreateTestCourseAsync(isPublished: false);

        // Act
        var response = await _client.PutAsync($"/api/courses/{course.Id}/publish", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
