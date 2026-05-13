using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

/// <summary>
/// Comprehensive integration tests for CourseRepository
/// Tests course creation, retrieval, pagination, filtering, soft delete, and advanced query scenarios
/// Task T172: Write CourseRepository integration tests with pagination edge cases, filtering combinations, soft delete behavior
/// </summary>
public class CourseRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CourseRepository _courseRepository;
    private readonly CategoryRepository _categoryRepository;

    // Test data
    private Guid _testCategoryId;
    private const string TestCourseTitle = "Bitcoin Fundamentals";
    private const string TestCourseDescription = "Learn Bitcoin basics and blockchain technology";

    public CourseRepositoryIntegrationTests()
    {
        // Setup in-memory database for each test
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _courseRepository = new CourseRepository(_context);
        _categoryRepository = new CategoryRepository(_context);

        // Create test category
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Cryptocurrency Basics",
            Description = "Learn crypto fundamentals",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _categoryRepository.AddAsync(category).Wait();
        _context.SaveChangesAsync().Wait();
        _testCategoryId = category.Id;
    }

    #region Course Creation Tests

    [Fact]
    public async Task AddAsync_WithValidCourse_ShouldPersistToDatabase()
    {
        // Arrange
        var course = CreateTestCourse(TestCourseTitle, TestCourseDescription);

        // Act
        await _courseRepository.AddAsync(course);
        await _context.SaveChangesAsync();

        // Assert
        var savedCourse = await _context.Set<Course>().FindAsync(course.Id);
        savedCourse.Should().NotBeNull();
        savedCourse!.Title.Should().Be(TestCourseTitle);
        savedCourse.Description.Should().Be(TestCourseDescription);
        savedCourse.DifficultyLevel.Should().Be(DifficultyLevel.Beginner);
        savedCourse.IsPremium.Should().BeFalse();
        savedCourse.IsPublished.Should().BeFalse();
        savedCourse.ViewCount.Should().Be(0);
        savedCourse.IsDeleted.Should().BeFalse();
        savedCourse.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddAsync_WithMultipleCourses_ShouldPersistAllToDatabase()
    {
        // Arrange
        var course1 = CreateTestCourse("Course 1", "Description 1");
        var course2 = CreateTestCourse("Course 2", "Description 2");
        var course3 = CreateTestCourse("Course 3", "Description 3");

        // Act
        await _courseRepository.AddAsync(course1);
        await _courseRepository.AddAsync(course2);
        await _courseRepository.AddAsync(course3);
        await _context.SaveChangesAsync();

        // Assert
        var allCourses = await _courseRepository.GetAllAsync();
        allCourses.Should().HaveCount(3);
        allCourses.Should().Contain(c => c.Title == "Course 1");
        allCourses.Should().Contain(c => c.Title == "Course 2");
        allCourses.Should().Contain(c => c.Title == "Course 3");
    }

    #endregion

    #region Course Retrieval Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingCourse_ShouldReturnCourse()
    {
        // Arrange
        var course = await CreateAndSaveTestCourseAsync();

        // Act
        var retrievedCourse = await _courseRepository.GetByIdAsync(course.Id);

        // Assert
        retrievedCourse.Should().NotBeNull();
        retrievedCourse!.Id.Should().Be(course.Id);
        retrievedCourse.Title.Should().Be(TestCourseTitle);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentCourse_ShouldReturnNull()
    {
        // Act
        var retrievedCourse = await _courseRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        retrievedCourse.Should().BeNull();
    }

    [Fact]
    public async Task GetWithLessonsAsync_WithCourseThatHasLessons_ShouldReturnCourseWithLessons()
    {
        // Arrange
        var course = await CreateAndSaveTestCourseAsync();
        var lesson1 = CreateLesson("Lesson 1", course.Id, 1);
        var lesson2 = CreateLesson("Lesson 2", course.Id, 2);
        var lesson3 = CreateLesson("Lesson 3", course.Id, 3);

        _context.Set<Lesson>().AddRange(lesson1, lesson2, lesson3);
        await _context.SaveChangesAsync();

        // Act
        var retrievedCourse = await _courseRepository.GetWithLessonsAsync(course.Id);

        // Assert
        retrievedCourse.Should().NotBeNull();
        retrievedCourse!.Lessons.Should().HaveCount(3);
        retrievedCourse.Lessons.Should().BeInAscendingOrder(l => l.OrderIndex);
        retrievedCourse.Lessons.First().Title.Should().Be("Lesson 1");
    }

    [Fact]
    public async Task GetWithLessonsAsync_WithCourseThatHasInactiveLessons_ShouldExcludeInactiveLessons()
    {
        // Arrange
        var course = await CreateAndSaveTestCourseAsync();
        var lesson1 = CreateLesson("Active Lesson", course.Id, 1);
        var lesson2 = CreateLesson("Inactive Lesson", course.Id, 2);
        lesson2.Deactivate(); // Mark as inactive

        _context.Set<Lesson>().AddRange(lesson1, lesson2);
        await _context.SaveChangesAsync();

        // Act
        var retrievedCourse = await _courseRepository.GetWithLessonsAsync(course.Id);

        // Assert
        retrievedCourse.Should().NotBeNull();
        retrievedCourse!.Lessons.Should().HaveCount(1);
        retrievedCourse.Lessons.First().Title.Should().Be("Active Lesson");
    }

    #endregion

    #region Pagination Tests (T172: Edge Cases)

    [Fact]
    public async Task GetByCategoryAsync_WithEmptyDatabase_ShouldReturnEmptyPage()
    {
        // Act
        var result = await _courseRepository.GetByCategoryAsync(_testCategoryId, 1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetByCategoryAsync_WithExactlyOnePageOfData_ShouldReturnAllItems()
    {
        // Arrange: Create exactly 10 published courses
        for (int i = 1; i <= 10; i++)
        {
            var course = CreateTestCourse($"Course {i}", $"Description {i}");
            course.IsPublished = true;
            await _courseRepository.AddAsync(course);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _courseRepository.GetByCategoryAsync(_testCategoryId, 1, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetByCategoryAsync_WithMultiplePages_ShouldHandleCorrectly()
    {
        // Arrange: Create 25 published courses (will span 3 pages with pageSize=10)
        for (int i = 1; i <= 25; i++)
        {
            var course = CreateTestCourse($"Course {i}", $"Description {i}");
            course.IsPublished = true;
            await _courseRepository.AddAsync(course);
        }
        await _context.SaveChangesAsync();

        // Act - Get page 1
        var page1 = await _courseRepository.GetByCategoryAsync(_testCategoryId, 1, 10);

        // Assert page 1
        page1.Items.Should().HaveCount(10);
        page1.TotalCount.Should().Be(25);
        page1.TotalPages.Should().Be(3);
        page1.HasNextPage.Should().BeTrue();
        page1.HasPreviousPage.Should().BeFalse();

        // Act - Get page 2
        var page2 = await _courseRepository.GetByCategoryAsync(_testCategoryId, 2, 10);

        // Assert page 2
        page2.Items.Should().HaveCount(10);
        page2.HasNextPage.Should().BeTrue();
        page2.HasPreviousPage.Should().BeTrue();

        // Act - Get page 3 (last page, partial)
        var page3 = await _courseRepository.GetByCategoryAsync(_testCategoryId, 3, 10);

        // Assert page 3
        page3.Items.Should().HaveCount(5); // Only 5 items on last page
        page3.HasNextPage.Should().BeFalse();
        page3.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCategoryAsync_WithPageBeyondTotalPages_ShouldReturnEmptyPage()
    {
        // Arrange: Create 5 courses
        for (int i = 1; i <= 5; i++)
        {
            var course = CreateTestCourse($"Course {i}", $"Description {i}");
            course.IsPublished = true;
            await _courseRepository.AddAsync(course);
        }
        await _context.SaveChangesAsync();

        // Act - Request page 5 when only page 1 exists
        var result = await _courseRepository.GetByCategoryAsync(_testCategoryId, 5, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
        result.PageNumber.Should().Be(5);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnOnlyPublishedCourses()
    {
        // Arrange
        var publishedCourse1 = CreateTestCourse("Published 1", "Description");
        publishedCourse1.IsPublished = true;

        var publishedCourse2 = CreateTestCourse("Published 2", "Description");
        publishedCourse2.IsPublished = true;

        var unpublishedCourse = CreateTestCourse("Unpublished", "Description");
        unpublishedCourse.IsPublished = false;

        await _courseRepository.AddAsync(publishedCourse1);
        await _courseRepository.AddAsync(publishedCourse2);
        await _courseRepository.AddAsync(unpublishedCourse);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courseRepository.GetByCategoryAsync(_testCategoryId, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(c => c.IsPublished.Should().BeTrue());
        result.Items.Should().NotContain(c => c.Title == "Unpublished");
    }

    #endregion

    #region Filtering Tests (T172: Filtering Combinations)

    [Fact]
    public async Task SearchCoursesAsync_WithNoFilters_ShouldReturnAllPublishedCourses()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var course = CreateTestCourse($"Course {i}", $"Description {i}");
            course.IsPublished = true;
            await _courseRepository.AddAsync(course);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _courseRepository.SearchCoursesAsync("", null, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task SearchCoursesAsync_WithSearchTerm_ShouldFilterByTitleAndDescription()
    {
        // Arrange
        var course1 = CreateTestCourse("Bitcoin Fundamentals", "Learn about Bitcoin");
        course1.IsPublished = true;

        var course2 = CreateTestCourse("Ethereum Smart Contracts", "Learn about smart contracts");
        course2.IsPublished = true;

        var course3 = CreateTestCourse("Bitcoin Mining", "Mining Bitcoin guide");
        course3.IsPublished = true;

        await _courseRepository.AddAsync(course1);
        await _courseRepository.AddAsync(course2);
        await _courseRepository.AddAsync(course3);
        await _context.SaveChangesAsync();

        // Act - Search for "Bitcoin"
        var result = await _courseRepository.SearchCoursesAsync("Bitcoin", null, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(c =>
            c.Title.Contains("Bitcoin") || c.Description.Contains("Bitcoin"));
    }

    [Fact]
    public async Task SearchCoursesAsync_WithDifficultyFilter_ShouldReturnOnlyMatchingDifficulty()
    {
        // Arrange
        var beginnerCourse = CreateTestCourse("Beginner Course", "Easy level");
        beginnerCourse.DifficultyLevel = DifficultyLevel.Beginner;
        beginnerCourse.IsPublished = true;

        var intermediateCourse = CreateTestCourse("Intermediate Course", "Medium level");
        intermediateCourse.DifficultyLevel = DifficultyLevel.Intermediate;
        intermediateCourse.IsPublished = true;

        var advancedCourse = CreateTestCourse("Advanced Course", "Hard level");
        advancedCourse.DifficultyLevel = DifficultyLevel.Advanced;
        advancedCourse.IsPublished = true;

        await _courseRepository.AddAsync(beginnerCourse);
        await _courseRepository.AddAsync(intermediateCourse);
        await _courseRepository.AddAsync(advancedCourse);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courseRepository.SearchCoursesAsync("", DifficultyLevel.Intermediate, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().DifficultyLevel.Should().Be(DifficultyLevel.Intermediate);
    }

    [Fact]
    public async Task SearchCoursesAsync_WithPremiumFilter_ShouldReturnOnlyPremiumCourses()
    {
        // Arrange
        var freeCourse1 = CreateTestCourse("Free Course 1", "Free content");
        freeCourse1.IsPremium = false;
        freeCourse1.IsPublished = true;

        var freeCourse2 = CreateTestCourse("Free Course 2", "Free content");
        freeCourse2.IsPremium = false;
        freeCourse2.IsPublished = true;

        var premiumCourse = CreateTestCourse("Premium Course", "Premium content");
        premiumCourse.IsPremium = true;
        premiumCourse.IsPublished = true;

        await _courseRepository.AddAsync(freeCourse1);
        await _courseRepository.AddAsync(freeCourse2);
        await _courseRepository.AddAsync(premiumCourse);
        await _context.SaveChangesAsync();

        // Act - Filter for premium courses
        var premiumResult = await _courseRepository.SearchCoursesAsync("", null, true, 1, 10);

        // Assert
        premiumResult.Items.Should().HaveCount(1);
        premiumResult.Items.First().IsPremium.Should().BeTrue();

        // Act - Filter for free courses
        var freeResult = await _courseRepository.SearchCoursesAsync("", null, false, 1, 10);

        // Assert
        freeResult.Items.Should().HaveCount(2);
        freeResult.Items.Should().AllSatisfy(c => c.IsPremium.Should().BeFalse());
    }

    [Fact]
    public async Task SearchCoursesAsync_WithMultipleFilters_ShouldApplyCombination()
    {
        // Arrange
        var matchingCourse = CreateTestCourse("Bitcoin Basics for Beginners", "Learn Bitcoin fundamentals");
        matchingCourse.DifficultyLevel = DifficultyLevel.Beginner;
        matchingCourse.IsPremium = false;
        matchingCourse.IsPublished = true;

        var nonMatchingCourse1 = CreateTestCourse("Ethereum Basics", "Learn Ethereum"); // No "Bitcoin"
        nonMatchingCourse1.DifficultyLevel = DifficultyLevel.Beginner;
        nonMatchingCourse1.IsPremium = false;
        nonMatchingCourse1.IsPublished = true;

        var nonMatchingCourse2 = CreateTestCourse("Advanced Bitcoin Trading", "Bitcoin trading"); // Advanced, not Beginner
        nonMatchingCourse2.DifficultyLevel = DifficultyLevel.Advanced;
        nonMatchingCourse2.IsPremium = false;
        nonMatchingCourse2.IsPublished = true;

        var nonMatchingCourse3 = CreateTestCourse("Bitcoin Premium Course", "Bitcoin for premium"); // Premium
        nonMatchingCourse3.DifficultyLevel = DifficultyLevel.Beginner;
        nonMatchingCourse3.IsPremium = true;
        nonMatchingCourse3.IsPublished = true;

        await _courseRepository.AddAsync(matchingCourse);
        await _courseRepository.AddAsync(nonMatchingCourse1);
        await _courseRepository.AddAsync(nonMatchingCourse2);
        await _courseRepository.AddAsync(nonMatchingCourse3);
        await _context.SaveChangesAsync();

        // Act - Search with all filters: "Bitcoin" + Beginner + Free
        var result = await _courseRepository.SearchCoursesAsync(
            "Bitcoin",
            DifficultyLevel.Beginner,
            false,
            1,
            10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Bitcoin Basics for Beginners");
        result.Items.First().DifficultyLevel.Should().Be(DifficultyLevel.Beginner);
        result.Items.First().IsPremium.Should().BeFalse();
    }

    [Fact]
    public async Task SearchCoursesAsync_WithCaseInsensitiveSearch_ShouldMatch()
    {
        // Arrange
        var course = CreateTestCourse("Bitcoin Fundamentals", "Learn about bitcoin blockchain");
        course.IsPublished = true;
        await _courseRepository.AddAsync(course);
        await _context.SaveChangesAsync();

        // Act - Search with different case variations
        var lowerCaseResult = await _courseRepository.SearchCoursesAsync("bitcoin", null, null, 1, 10);
        var upperCaseResult = await _courseRepository.SearchCoursesAsync("BITCOIN", null, null, 1, 10);
        var mixedCaseResult = await _courseRepository.SearchCoursesAsync("BiTcOiN", null, null, 1, 10);

        // Assert
        lowerCaseResult.Items.Should().HaveCount(1);
        upperCaseResult.Items.Should().HaveCount(1);
        mixedCaseResult.Items.Should().HaveCount(1);
    }

    #endregion

    #region Soft Delete Tests (T172: Soft Delete Behavior)

    [Fact]
    public async Task GetAllAsync_ShouldExcludeSoftDeletedCourses()
    {
        // Arrange
        var activeCourse = CreateTestCourse("Active Course", "Active");
        var deletedCourse = CreateTestCourse("Deleted Course", "Deleted");
        deletedCourse.IsDeleted = true; // Soft delete

        await _courseRepository.AddAsync(activeCourse);
        await _courseRepository.AddAsync(deletedCourse);
        await _context.SaveChangesAsync();

        // Act
        var allCourses = await _courseRepository.GetAllAsync();

        // Assert
        allCourses.Should().HaveCount(1);
        allCourses.First().Title.Should().Be("Active Course");
    }

    [Fact]
    public async Task GetByIdAsync_WithSoftDeletedCourse_ShouldReturnNull()
    {
        // Arrange
        var course = CreateTestCourse("Test Course", "Description");
        await _courseRepository.AddAsync(course);
        await _context.SaveChangesAsync();

        // Soft delete the course
        course.SoftDelete();
        _context.Entry(course).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Act
        var retrievedCourse = await _courseRepository.GetByIdAsync(course.Id);

        // Assert
        retrievedCourse.Should().BeNull();
    }

    [Fact]
    public async Task SearchCoursesAsync_ShouldExcludeSoftDeletedCourses()
    {
        // Arrange
        var activeCourse = CreateTestCourse("Active Bitcoin Course", "Active");
        activeCourse.IsPublished = true;

        var deletedCourse = CreateTestCourse("Deleted Bitcoin Course", "Deleted");
        deletedCourse.IsPublished = true;
        deletedCourse.IsDeleted = true;

        await _courseRepository.AddAsync(activeCourse);
        await _courseRepository.AddAsync(deletedCourse);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courseRepository.SearchCoursesAsync("Bitcoin", null, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Active Bitcoin Course");
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldExcludeSoftDeletedCourses()
    {
        // Arrange
        var activeCourse = CreateTestCourse("Active Course", "Active");
        activeCourse.IsPublished = true;

        var deletedCourse = CreateTestCourse("Deleted Course", "Deleted");
        deletedCourse.IsPublished = true;
        deletedCourse.IsDeleted = true;

        await _courseRepository.AddAsync(activeCourse);
        await _courseRepository.AddAsync(deletedCourse);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courseRepository.GetByCategoryAsync(_testCategoryId, 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Active Course");
    }

    #endregion

    #region Advanced Query Tests

    [Fact]
    public async Task GetAllCoursesForAdminAsync_ShouldReturnAllCoursesIncludingUnpublished()
    {
        // Arrange
        var publishedCourse = CreateTestCourse("Published Course", "Published");
        publishedCourse.IsPublished = true;

        var unpublishedCourse = CreateTestCourse("Unpublished Course", "Unpublished");
        unpublishedCourse.IsPublished = false;

        await _courseRepository.AddAsync(publishedCourse);
        await _courseRepository.AddAsync(unpublishedCourse);
        await _context.SaveChangesAsync();

        // Act
        var result = await _courseRepository.GetAllCoursesForAdminAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(c => c.Title == "Published Course");
        result.Items.Should().Contain(c => c.Title == "Unpublished Course");
    }

    [Fact(Skip = "Requires PostgreSQL - ExecuteSqlInterpolated not supported by InMemoryDatabase")]
    public async Task IncrementViewCountAsync_ShouldIncreaseViewCount()
    {
        // Arrange
        var course = CreateTestCourse("Test Course", "Description");
        await _courseRepository.AddAsync(course);
        await _context.SaveChangesAsync();

        var initialViewCount = course.ViewCount;

        // Act
        await _courseRepository.IncrementViewCountAsync(course.Id);

        // Assert
        var updatedCourse = await _context.Set<Course>().FindAsync(course.Id);
        updatedCourse.Should().NotBeNull();
        updatedCourse!.ViewCount.Should().Be(initialViewCount + 1);
    }

    [Fact(Skip = "Requires PostgreSQL - ExecuteSqlInterpolated not supported by InMemoryDatabase")]
    public async Task IncrementViewCountAsync_CalledMultipleTimes_ShouldIncrementCorrectly()
    {
        // Arrange
        var course = CreateTestCourse("Test Course", "Description");
        await _courseRepository.AddAsync(course);
        await _context.SaveChangesAsync();

        // Act - Increment 5 times
        for (int i = 0; i < 5; i++)
        {
            await _courseRepository.IncrementViewCountAsync(course.Id);
        }

        // Assert
        var updatedCourse = await _context.Set<Course>().FindAsync(course.Id);
        updatedCourse.Should().NotBeNull();
        updatedCourse!.ViewCount.Should().Be(5);
    }

    [Fact]
    public async Task IsUserEnrolledAsync_WithEnrolledUser_ShouldReturnTrue()
    {
        // Arrange
        var course = await CreateAndSaveTestCourseAsync();
        var userId = Guid.NewGuid();

        var enrollment = new UserCourseEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow,
            CompletionPercentage = 0
        };

        _context.Set<UserCourseEnrollment>().Add(enrollment);
        await _context.SaveChangesAsync();

        // Act
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(userId, course.Id);

        // Assert
        isEnrolled.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserEnrolledAsync_WithNonEnrolledUser_ShouldReturnFalse()
    {
        // Arrange
        var course = await CreateAndSaveTestCourseAsync();
        var userId = Guid.NewGuid();

        // Act
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(userId, course.Id);

        // Assert
        isEnrolled.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private Course CreateTestCourse(string title, string description)
    {
        return new Course
        {
            Id = Guid.NewGuid(),
            CategoryId = _testCategoryId,
            Title = title,
            Description = description,
            DifficultyLevel = DifficultyLevel.Beginner,
            EstimatedDuration = 60,
            IsPremium = false,
            RewardPoints = 100,
            IsPublished = false,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
            Lessons = new List<Lesson>()
        };
    }

    private async Task<Course> CreateAndSaveTestCourseAsync()
    {
        var course = CreateTestCourse(TestCourseTitle, TestCourseDescription);
        await _courseRepository.AddAsync(course);
        await _context.SaveChangesAsync();
        return course;
    }

    private Lesson CreateLesson(string title, Guid courseId, int orderIndex)
    {
        return new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            Description = "Test lesson description",
            YouTubeVideoId = "dQw4w9WgXcQ", // Valid YouTube ID format
            Duration = 600, // 10 minutes
            OrderIndex = orderIndex,
            IsPremium = false,
            RewardPoints = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
