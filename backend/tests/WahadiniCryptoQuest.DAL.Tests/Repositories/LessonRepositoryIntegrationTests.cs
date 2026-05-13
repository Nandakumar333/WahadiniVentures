using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

/// <summary>
/// Comprehensive integration tests for LessonRepository
/// Tests lesson creation, retrieval, reordering logic, OrderIndex gap handling, and soft delete behavior
/// Task T173: Write LessonRepository integration tests: reorder logic, OrderIndex gaps handled
/// </summary>
public class LessonRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LessonRepository _lessonRepository;
    private readonly CourseRepository _courseRepository;
    private readonly CategoryRepository _categoryRepository;

    // Test data
    private Guid _testCourseId;
    private const string TestLessonTitle = "Introduction to Bitcoin";
    private const string TestYouTubeVideoId = "dQw4w9WgXcQ"; // Valid 11-character format

    public LessonRepositoryIntegrationTests()
    {
        // Setup in-memory database for each test
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _lessonRepository = new LessonRepository(_context);
        _courseRepository = new CourseRepository(_context);
        _categoryRepository = new CategoryRepository(_context);

        // Create test category and course
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Cryptocurrency",
            Description = "Crypto courses",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _categoryRepository.AddAsync(category).Wait();
        _context.SaveChangesAsync().Wait();

        var course = new Course
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Title = "Bitcoin Fundamentals",
            Description = "Learn Bitcoin",
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPublished = true
        };
        _courseRepository.AddAsync(course).Wait();
        _context.SaveChangesAsync().Wait();
        _testCourseId = course.Id;
    }

    #region Lesson Creation Tests

    [Fact]
    public async Task AddAsync_WithValidLesson_ShouldPersistToDatabase()
    {
        // Arrange
        var lesson = CreateTestLesson(TestLessonTitle, 1);

        // Act
        await _lessonRepository.AddAsync(lesson);
        await _context.SaveChangesAsync();

        // Assert
        var savedLesson = await _context.Set<Lesson>().FindAsync(lesson.Id);
        savedLesson.Should().NotBeNull();
        savedLesson!.Title.Should().Be(TestLessonTitle);
        savedLesson.YouTubeVideoId.Should().Be(TestYouTubeVideoId);
        savedLesson.OrderIndex.Should().Be(1);
        savedLesson.IsActive.Should().BeTrue();
        savedLesson.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddAsync_WithMultipleLessons_ShouldPersistAllToDatabase()
    {
        // Arrange
        var lesson1 = CreateTestLesson("Lesson 1", 1);
        var lesson2 = CreateTestLesson("Lesson 2", 2);
        var lesson3 = CreateTestLesson("Lesson 3", 3);

        // Act
        await _lessonRepository.AddAsync(lesson1);
        await _lessonRepository.AddAsync(lesson2);
        await _lessonRepository.AddAsync(lesson3);
        await _context.SaveChangesAsync();

        // Assert
        var allLessons = await _lessonRepository.GetAllAsync();
        allLessons.Should().HaveCount(3);
    }

    #endregion

    #region Lesson Retrieval Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingLesson_ShouldReturnLesson()
    {
        // Arrange
        var lesson = await CreateAndSaveTestLessonAsync("Test Lesson", 1);

        // Act
        var retrievedLesson = await _lessonRepository.GetByIdAsync(lesson.Id);

        // Assert
        retrievedLesson.Should().NotBeNull();
        retrievedLesson!.Id.Should().Be(lesson.Id);
        retrievedLesson.Title.Should().Be("Test Lesson");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentLesson_ShouldReturnNull()
    {
        // Act
        var retrievedLesson = await _lessonRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        retrievedLesson.Should().BeNull();
    }

    [Fact]
    public async Task GetByCourseIdOrderedAsync_ShouldReturnLessonsOrderedByOrderIndex()
    {
        // Arrange - Add lessons in non-sequential order
        var lesson3 = CreateTestLesson("Lesson 3", 3);
        var lesson1 = CreateTestLesson("Lesson 1", 1);
        var lesson5 = CreateTestLesson("Lesson 5", 5);
        var lesson2 = CreateTestLesson("Lesson 2", 2);

        await _lessonRepository.AddAsync(lesson3);
        await _lessonRepository.AddAsync(lesson1);
        await _lessonRepository.AddAsync(lesson5);
        await _lessonRepository.AddAsync(lesson2);
        await _context.SaveChangesAsync();

        // Act
        var orderedLessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();

        // Assert
        orderedLessons.Should().HaveCount(4);
        orderedLessons.Should().BeInAscendingOrder(l => l.OrderIndex);
        orderedLessons[0].OrderIndex.Should().Be(1);
        orderedLessons[1].OrderIndex.Should().Be(2);
        orderedLessons[2].OrderIndex.Should().Be(3);
        orderedLessons[3].OrderIndex.Should().Be(5);
    }

    [Fact]
    public async Task GetByCourseIdOrderedAsync_WithEmptyCourse_ShouldReturnEmptyList()
    {
        // Act
        var lessons = await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId);

        // Assert
        lessons.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLessonWithCourseAsync_ShouldIncludeCourseData()
    {
        // Arrange
        var lesson = await CreateAndSaveTestLessonAsync("Test Lesson", 1);

        // Act
        var lessonWithCourse = await _lessonRepository.GetLessonWithCourseAsync(lesson.Id);

        // Assert
        lessonWithCourse.Should().NotBeNull();
        lessonWithCourse!.Course.Should().NotBeNull();
        lessonWithCourse.Course.Title.Should().Be("Bitcoin Fundamentals");
    }

    #endregion

    #region Navigation Tests (Next/Previous Lesson)

    [Fact]
    public async Task GetNextLessonAsync_WithLessonsAvailable_ShouldReturnNextLesson()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 3);

        // Act
        var nextLesson = await _lessonRepository.GetNextLessonAsync(_testCourseId, lesson1.OrderIndex);

        // Assert
        nextLesson.Should().NotBeNull();
        nextLesson!.Id.Should().Be(lesson2.Id);
        nextLesson.OrderIndex.Should().Be(2);
    }

    [Fact]
    public async Task GetNextLessonAsync_FromLastLesson_ShouldReturnNull()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);

        // Act
        var nextLesson = await _lessonRepository.GetNextLessonAsync(_testCourseId, lesson2.OrderIndex);

        // Assert
        nextLesson.Should().BeNull();
    }

    [Fact]
    public async Task GetPreviousLessonAsync_WithLessonsAvailable_ShouldReturnPreviousLesson()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 3);

        // Act
        var previousLesson = await _lessonRepository.GetPreviousLessonAsync(_testCourseId, lesson3.OrderIndex);

        // Assert
        previousLesson.Should().NotBeNull();
        previousLesson!.Id.Should().Be(lesson2.Id);
        previousLesson.OrderIndex.Should().Be(2);
    }

    [Fact]
    public async Task GetPreviousLessonAsync_FromFirstLesson_ShouldReturnNull()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);

        // Act
        var previousLesson = await _lessonRepository.GetPreviousLessonAsync(_testCourseId, lesson1.OrderIndex);

        // Assert
        previousLesson.Should().BeNull();
    }

    #endregion

    #region Reorder Logic Tests (T173: Comprehensive Reordering)

    [Fact]
    public async Task ReorderLessonsAsync_WithSequentialReorder_ShouldUpdateOrderIndexes()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 3);

        // Act - Reverse order: Lesson 3 becomes first
        var reorderMap = new Dictionary<Guid, int>
        {
            { lesson3.Id, 1 },
            { lesson2.Id, 2 },
            { lesson1.Id, 3 }
        };

        await _lessonRepository.ReorderLessonsAsync(_testCourseId, reorderMap);

        // Assert
        var reorderedLessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();
        reorderedLessons[0].Id.Should().Be(lesson3.Id);
        reorderedLessons[0].OrderIndex.Should().Be(1);
        reorderedLessons[1].Id.Should().Be(lesson2.Id);
        reorderedLessons[1].OrderIndex.Should().Be(2);
        reorderedLessons[2].Id.Should().Be(lesson1.Id);
        reorderedLessons[2].OrderIndex.Should().Be(3);
    }

    [Fact]
    public async Task ReorderLessonsAsync_WithPartialReorder_ShouldUpdateOnlySpecifiedLessons()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 3);

        // Act - Swap only lesson 1 and lesson 2, leave lesson 3 unchanged
        var reorderMap = new Dictionary<Guid, int>
        {
            { lesson2.Id, 1 },
            { lesson1.Id, 2 }
            // lesson3 not included
        };

        await _lessonRepository.ReorderLessonsAsync(_testCourseId, reorderMap);

        // Assert
        var reorderedLessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();
        reorderedLessons[0].Id.Should().Be(lesson2.Id);
        reorderedLessons[0].OrderIndex.Should().Be(1);
        reorderedLessons[1].Id.Should().Be(lesson1.Id);
        reorderedLessons[1].OrderIndex.Should().Be(2);
        reorderedLessons[2].Id.Should().Be(lesson3.Id);
        reorderedLessons[2].OrderIndex.Should().Be(3); // Unchanged
    }

    [Fact]
    public async Task ReorderLessonsAsync_WithInsertAtBeginning_ShouldShiftOthersDown()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 3);

        // Act - Move lesson 3 to position 1
        var reorderMap = new Dictionary<Guid, int>
        {
            { lesson3.Id, 1 },
            { lesson1.Id, 2 },
            { lesson2.Id, 3 }
        };

        await _lessonRepository.ReorderLessonsAsync(_testCourseId, reorderMap);

        // Assert
        var reorderedLessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();
        reorderedLessons[0].Title.Should().Be("Lesson 3");
        reorderedLessons[1].Title.Should().Be("Lesson 1");
        reorderedLessons[2].Title.Should().Be("Lesson 2");
    }

    [Fact]
    public async Task ReorderLessonsAsync_WithMoveToEnd_ShouldRepositionCorrectly()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 3);

        // Act - Move lesson 1 to last position
        var reorderMap = new Dictionary<Guid, int>
        {
            { lesson2.Id, 1 },
            { lesson3.Id, 2 },
            { lesson1.Id, 3 }
        };

        await _lessonRepository.ReorderLessonsAsync(_testCourseId, reorderMap);

        // Assert
        var reorderedLessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();
        reorderedLessons[0].Title.Should().Be("Lesson 2");
        reorderedLessons[1].Title.Should().Be("Lesson 3");
        reorderedLessons[2].Title.Should().Be("Lesson 1");
    }

    [Fact]
    public async Task ReorderLessonsAsync_WithComplexShuffle_ShouldHandleCorrectly()
    {
        // Arrange - Create 5 lessons
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 3);
        var lesson4 = await CreateAndSaveTestLessonAsync("Lesson 4", 4);
        var lesson5 = await CreateAndSaveTestLessonAsync("Lesson 5", 5);

        // Act - Complex reorder: [1,2,3,4,5] -> [5,3,1,4,2]
        var reorderMap = new Dictionary<Guid, int>
        {
            { lesson5.Id, 1 },
            { lesson3.Id, 2 },
            { lesson1.Id, 3 },
            { lesson4.Id, 4 },
            { lesson2.Id, 5 }
        };

        await _lessonRepository.ReorderLessonsAsync(_testCourseId, reorderMap);

        // Assert
        var reorderedLessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();
        reorderedLessons[0].Title.Should().Be("Lesson 5");
        reorderedLessons[1].Title.Should().Be("Lesson 3");
        reorderedLessons[2].Title.Should().Be("Lesson 1");
        reorderedLessons[3].Title.Should().Be("Lesson 4");
        reorderedLessons[4].Title.Should().Be("Lesson 2");
    }

    #endregion

    #region OrderIndex Gap Handling Tests (T173)

    [Fact]
    public async Task GetByCourseIdOrderedAsync_WithGapsInOrderIndex_ShouldMaintainOrder()
    {
        // Arrange - Create lessons with gaps: OrderIndex 1, 5, 10, 15
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 5);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 10);
        var lesson4 = await CreateAndSaveTestLessonAsync("Lesson 4", 15);

        // Act
        var orderedLessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();

        // Assert
        orderedLessons.Should().HaveCount(4);
        orderedLessons.Should().BeInAscendingOrder(l => l.OrderIndex);
        orderedLessons[0].OrderIndex.Should().Be(1);
        orderedLessons[1].OrderIndex.Should().Be(5);
        orderedLessons[2].OrderIndex.Should().Be(10);
        orderedLessons[3].OrderIndex.Should().Be(15);
    }

    [Fact]
    public async Task GetNextLessonAsync_WithGapsInOrderIndex_ShouldReturnCorrectNextLesson()
    {
        // Arrange - OrderIndex: 1, 10, 20
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 10);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 20);

        // Act
        var nextAfter1 = await _lessonRepository.GetNextLessonAsync(_testCourseId, 1);
        var nextAfter10 = await _lessonRepository.GetNextLessonAsync(_testCourseId, 10);

        // Assert
        nextAfter1.Should().NotBeNull();
        nextAfter1!.OrderIndex.Should().Be(10); // Jump from 1 to 10
        nextAfter10.Should().NotBeNull();
        nextAfter10!.OrderIndex.Should().Be(20); // Jump from 10 to 20
    }

    [Fact]
    public async Task GetPreviousLessonAsync_WithGapsInOrderIndex_ShouldReturnCorrectPreviousLesson()
    {
        // Arrange - OrderIndex: 1, 10, 20
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 10);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 20);

        // Act
        var prevBefore20 = await _lessonRepository.GetPreviousLessonAsync(_testCourseId, 20);
        var prevBefore10 = await _lessonRepository.GetPreviousLessonAsync(_testCourseId, 10);

        // Assert
        prevBefore20.Should().NotBeNull();
        prevBefore20!.OrderIndex.Should().Be(10); // Jump from 20 to 10
        prevBefore10.Should().NotBeNull();
        prevBefore10!.OrderIndex.Should().Be(1); // Jump from 10 to 1
    }

    [Fact]
    public async Task ReorderLessonsAsync_WithGappedIndexes_ShouldWorkCorrectly()
    {
        // Arrange - Start with gapped indexes: 5, 10, 15
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 5);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 10);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 15);

        // Act - Reorder to new gapped sequence: 2, 7, 12
        var reorderMap = new Dictionary<Guid, int>
        {
            { lesson1.Id, 2 },
            { lesson2.Id, 7 },
            { lesson3.Id, 12 }
        };

        await _lessonRepository.ReorderLessonsAsync(_testCourseId, reorderMap);

        // Assert
        var reorderedLessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();
        reorderedLessons[0].OrderIndex.Should().Be(2);
        reorderedLessons[1].OrderIndex.Should().Be(7);
        reorderedLessons[2].OrderIndex.Should().Be(12);
    }

    [Fact]
    public async Task GetMaxOrderIndexAsync_WithGappedIndexes_ShouldReturnMaximum()
    {
        // Arrange - OrderIndex: 3, 8, 20
        await CreateAndSaveTestLessonAsync("Lesson 1", 3);
        await CreateAndSaveTestLessonAsync("Lesson 2", 8);
        await CreateAndSaveTestLessonAsync("Lesson 3", 20);

        // Act
        var maxOrderIndex = await _lessonRepository.GetMaxOrderIndexAsync(_testCourseId);

        // Assert
        maxOrderIndex.Should().Be(20);
    }

    [Fact]
    public async Task GetMaxOrderIndexAsync_WithEmptyCourse_ShouldReturnZero()
    {
        // Act
        var maxOrderIndex = await _lessonRepository.GetMaxOrderIndexAsync(_testCourseId);

        // Assert
        maxOrderIndex.Should().Be(0);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public async Task GetAllAsync_ShouldExcludeInactiveLessons()
    {
        // Arrange
        var activeLesson = await CreateAndSaveTestLessonAsync("Active Lesson", 1);
        var inactiveLesson = await CreateAndSaveTestLessonAsync("Inactive Lesson", 2);
        inactiveLesson.Deactivate();
        _context.Entry(inactiveLesson).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Act
        var allLessons = await _lessonRepository.GetAllAsync();

        // Assert
        allLessons.Should().HaveCount(1);
        allLessons.First().Title.Should().Be("Active Lesson");
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveLesson_ShouldReturnNull()
    {
        // Arrange
        var lesson = await CreateAndSaveTestLessonAsync("Test Lesson", 1);
        lesson.Deactivate();
        _context.Entry(lesson).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Act
        var retrievedLesson = await _lessonRepository.GetByIdAsync(lesson.Id);

        // Assert
        retrievedLesson.Should().BeNull();
    }

    [Fact]
    public async Task GetByCourseIdOrderedAsync_ShouldExcludeInactiveLessons()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 3);

        // Mark lesson 2 as inactive
        lesson2.Deactivate();
        _context.Entry(lesson2).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Act
        var activeLessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();

        // Assert
        activeLessons.Should().HaveCount(2);
        activeLessons.Should().NotContain(l => l.Title == "Lesson 2");
    }

    [Fact]
    public async Task GetNextLessonAsync_ShouldSkipInactiveLessons()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2); // Will be inactive
        var lesson3 = await CreateAndSaveTestLessonAsync("Lesson 3", 3);

        // Mark lesson 2 as inactive
        lesson2.Deactivate();
        _context.Entry(lesson2).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Act
        var nextLesson = await _lessonRepository.GetNextLessonAsync(_testCourseId, lesson1.OrderIndex);

        // Assert
        nextLesson.Should().NotBeNull();
        nextLesson!.Id.Should().Be(lesson3.Id); // Should skip lesson 2
        nextLesson.OrderIndex.Should().Be(3);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ReorderLessonsAsync_WithEmptyMap_ShouldNotChangeAnything()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);
        var lesson2 = await CreateAndSaveTestLessonAsync("Lesson 2", 2);

        // Act
        var emptyReorderMap = new Dictionary<Guid, int>();
        await _lessonRepository.ReorderLessonsAsync(_testCourseId, emptyReorderMap);

        // Assert
        var lessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();
        lessons[0].OrderIndex.Should().Be(1);
        lessons[1].OrderIndex.Should().Be(2);
    }

    [Fact]
    public async Task ReorderLessonsAsync_WithNonExistentLessonId_ShouldIgnoreInvalidIds()
    {
        // Arrange
        var lesson1 = await CreateAndSaveTestLessonAsync("Lesson 1", 1);

        // Act - Include a non-existent lesson ID in reorder map
        var reorderMap = new Dictionary<Guid, int>
        {
            { lesson1.Id, 5 },
            { Guid.NewGuid(), 1 } // Non-existent lesson ID
        };

        await _lessonRepository.ReorderLessonsAsync(_testCourseId, reorderMap);

        // Assert - Should only update existing lesson
        var lesson = await _lessonRepository.GetByIdAsync(lesson1.Id);
        lesson!.OrderIndex.Should().Be(5);
    }

    [Fact]
    public async Task GetByCourseIdOrderedAsync_WithDuplicateOrderIndexes_ShouldReturnAllLessons()
    {
        // Arrange - Create lessons with duplicate OrderIndex (edge case)
        var lesson1 = CreateTestLesson("Lesson 1", 1);
        var lesson2 = CreateTestLesson("Lesson 2", 1); // Duplicate
        var lesson3 = CreateTestLesson("Lesson 3", 2);

        await _lessonRepository.AddAsync(lesson1);
        await _lessonRepository.AddAsync(lesson2);
        await _lessonRepository.AddAsync(lesson3);
        await _context.SaveChangesAsync();

        // Act
        var lessons = (await _lessonRepository.GetByCourseIdOrderedAsync(_testCourseId)).ToList();

        // Assert - All lessons should be returned, ordered by OrderIndex
        lessons.Should().HaveCount(3);
        lessons.Should().BeInAscendingOrder(l => l.OrderIndex);
    }

    #endregion

    #region Helper Methods

    private Lesson CreateTestLesson(string title, int orderIndex)
    {
        return new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = _testCourseId,
            Title = title,
            Description = "Test lesson description",
            YouTubeVideoId = TestYouTubeVideoId,
            Duration = 600, // 10 minutes
            OrderIndex = orderIndex,
            IsPremium = false,
            RewardPoints = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<Lesson> CreateAndSaveTestLessonAsync(string title, int orderIndex)
    {
        var lesson = CreateTestLesson(title, orderIndex);
        await _lessonRepository.AddAsync(lesson);
        await _context.SaveChangesAsync();
        return lesson;
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
