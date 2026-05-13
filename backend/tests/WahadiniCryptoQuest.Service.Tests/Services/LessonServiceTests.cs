using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Lesson;

namespace WahadiniCryptoQuest.Service.Tests.Services;

/// <summary>
/// Unit tests for LessonService
/// Tests cover: CreateLessonAsync, UpdateLessonAsync, DeleteLessonAsync, ReorderLessonsAsync
/// </summary>
public class LessonServiceTests
{
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<LessonService>> _loggerMock;
    private readonly ILessonService _lessonService;

    public LessonServiceTests()
    {
        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<LessonService>>();

        _lessonService = new LessonService(
            _lessonRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region ReorderLessonsAsync Tests (T117)

    [Fact]
    public async Task ReorderLessonsAsync_UpdatesOrderIndexCorrectly()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lesson1 = CreateTestLesson(courseId, "Lesson 1", 1);
        var lesson2 = CreateTestLesson(courseId, "Lesson 2", 2);
        var lesson3 = CreateTestLesson(courseId, "Lesson 3", 3);
        var lesson4 = CreateTestLesson(courseId, "Lesson 4", 4);

        var lessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>
        {
            lesson1, lesson2, lesson3, lesson4
        };

        var lessonOrderMap = new Dictionary<Guid, int>
        {
            { lesson3.Id, 1 }, // Move lesson 3 to position 1
            { lesson1.Id, 2 }, // Lesson 1 to position 2
            { lesson2.Id, 3 }, // Lesson 2 to position 3
            { lesson4.Id, 4 }  // Lesson 4 stays at position 4
        };

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            CategoryId = Guid.NewGuid()
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.GetByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        var updatedLessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>();
        _lessonRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Lesson>()))
            .Callback<WahadiniCryptoQuest.Core.Entities.Lesson>(l => updatedLessons.Add(l))
            .ReturnsAsync((WahadiniCryptoQuest.Core.Entities.Lesson l) => l);

        // Act
        await _lessonService.ReorderLessonsAsync(courseId, lessonOrderMap);

        // Assert
        _lessonRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Lesson>()),
            Times.Exactly(4));

        // Verify order indices are updated correctly
        var updatedLesson1 = updatedLessons.First(l => l.Id == lesson1.Id);
        var updatedLesson2 = updatedLessons.First(l => l.Id == lesson2.Id);
        var updatedLesson3 = updatedLessons.First(l => l.Id == lesson3.Id);
        var updatedLesson4 = updatedLessons.First(l => l.Id == lesson4.Id);

        updatedLesson3.OrderIndex.Should().Be(1);
        updatedLesson1.OrderIndex.Should().Be(2);
        updatedLesson2.OrderIndex.Should().Be(3);
        updatedLesson4.OrderIndex.Should().Be(4);
    }

    [Fact]
    public async Task ReorderLessonsAsync_EnsuresNoGapsInSequence()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lesson1 = CreateTestLesson(courseId, "Lesson 1", 1);
        var lesson2 = CreateTestLesson(courseId, "Lesson 2", 5); // Gap in sequence
        var lesson3 = CreateTestLesson(courseId, "Lesson 3", 10); // Another gap

        var lessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>
        {
            lesson1, lesson2, lesson3
        };

        var lessonOrderMap = new Dictionary<Guid, int>
        {
            { lesson1.Id, 1 },
            { lesson2.Id, 2 },
            { lesson3.Id, 3 }
        };

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            CategoryId = Guid.NewGuid()
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.GetByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        var updatedLessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>();
        _lessonRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Lesson>()))
            .Callback<WahadiniCryptoQuest.Core.Entities.Lesson>(l => updatedLessons.Add(l))
            .ReturnsAsync((WahadiniCryptoQuest.Core.Entities.Lesson l) => l);

        // Act
        await _lessonService.ReorderLessonsAsync(courseId, lessonOrderMap);

        // Assert
        var orderIndices = updatedLessons.Select(l => l.OrderIndex).OrderBy(i => i).ToList();

        // Should be gap-free: 1, 2, 3
        orderIndices.Should().Equal(1, 2, 3);
        orderIndices.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task ReorderLessonsAsync_WithDragDropScenario_UpdatesCorrectly()
    {
        // Arrange - Simulate dragging lesson 5 to position 1
        var courseId = Guid.NewGuid();
        var lessons = Enumerable.Range(1, 5)
            .Select(i => CreateTestLesson(courseId, $"Lesson {i}", i))
            .ToList();

        // Drag lesson 5 to position 1: New order should be [5, 1, 2, 3, 4]
        var lessonOrderMap = new Dictionary<Guid, int>
        {
            { lessons[4].Id, 1 },
            { lessons[0].Id, 2 },
            { lessons[1].Id, 3 },
            { lessons[2].Id, 4 },
            { lessons[3].Id, 5 }
        };

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            CategoryId = Guid.NewGuid()
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.GetByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        var updatedLessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>();
        _lessonRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Lesson>()))
            .Callback<WahadiniCryptoQuest.Core.Entities.Lesson>(l => updatedLessons.Add(l))
            .ReturnsAsync((WahadiniCryptoQuest.Core.Entities.Lesson l) => l);

        // Act
        await _lessonService.ReorderLessonsAsync(courseId, lessonOrderMap);

        // Assert
        var lesson5 = updatedLessons.First(l => l.Title == "Lesson 5");
        lesson5.OrderIndex.Should().Be(1);

        var lesson1 = updatedLessons.First(l => l.Title == "Lesson 1");
        lesson1.OrderIndex.Should().Be(2);
    }

    [Fact]
    public async Task ReorderLessonsAsync_EmptyList_DoesNothing()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var emptyOrderMap = new Dictionary<Guid, int>();

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            CategoryId = Guid.NewGuid()
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.GetByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WahadiniCryptoQuest.Core.Entities.Lesson>());

        // Act
        await _lessonService.ReorderLessonsAsync(courseId, emptyOrderMap);

        // Assert
        _lessonRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Lesson>()),
            Times.Never);
    }

    [Fact]
    public async Task ReorderLessonsAsync_InvalidLessonId_ThrowsInvalidOperationException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lesson1 = CreateTestLesson(courseId, "Lesson 1", 1);

        var invalidOrderMap = new Dictionary<Guid, int>
        {
            { Guid.NewGuid(), 1 } // Invalid lesson ID
        };

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            CategoryId = Guid.NewGuid()
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.GetByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WahadiniCryptoQuest.Core.Entities.Lesson> { lesson1 });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _lessonService.ReorderLessonsAsync(courseId, invalidOrderMap));
    }

    #endregion

    #region CreateLessonAsync Tests

    [Fact]
    public async Task CreateLessonAsync_ValidLesson_AutoIncrementsOrderIndex()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingLessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>
        {
            CreateTestLesson(courseId, "Lesson 1", 1),
            CreateTestLesson(courseId, "Lesson 2", 2)
        };

        var createDto = new CreateLessonDto
        {
            CourseId = courseId,
            Title = "New Lesson",
            Description = "Description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 15,
            RewardPoints = 100
        };

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            CategoryId = Guid.NewGuid()
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var lessonEntity = new WahadiniCryptoQuest.Core.Entities.Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = createDto.Title,
            Description = createDto.Description,
            YouTubeVideoId = createDto.YouTubeVideoId,
            Duration = createDto.Duration,
            OrderIndex = 3,
            RewardPoints = createDto.RewardPoints
        };

        _mapperMock
            .Setup(x => x.Map<WahadiniCryptoQuest.Core.Entities.Lesson>(createDto))
            .Returns(lessonEntity);

        _lessonRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Lesson>()))
            .ReturnsAsync((WahadiniCryptoQuest.Core.Entities.Lesson l) => l);

        var lessonDto = new LessonDto
        {
            Id = lessonEntity.Id,
            Title = lessonEntity.Title,
            OrderIndex = lessonEntity.OrderIndex
        };

        _mapperMock
            .Setup(x => x.Map<LessonDto>(It.IsAny<WahadiniCryptoQuest.Core.Entities.Lesson>()))
            .Returns(lessonDto);

        // Act
        var result = await _lessonService.CreateLessonAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.OrderIndex.Should().Be(3); // Should be max(2) + 1
    }

    [Fact]
    public async Task CreateLessonAsync_InvalidYouTubeId_ThrowsArgumentException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var createDto = new CreateLessonDto
        {
            CourseId = courseId,
            Title = "New Lesson",
            Description = "Description",
            YouTubeVideoId = "invalid", // Invalid: not 11 chars
            Duration = 15,
            RewardPoints = 100
        };

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            CategoryId = Guid.NewGuid()
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _lessonService.CreateLessonAsync(createDto));
    }

    #endregion

    #region DeleteLessonAsync Tests

    [Fact]
    public async Task DeleteLessonAsync_SoftDeletesLesson()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var lesson = new WahadiniCryptoQuest.Core.Entities.Lesson
        {
            Id = lessonId,
            CourseId = Guid.NewGuid(),
            Title = "Test Lesson",
            Description = "Test Description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 15,
            OrderIndex = 1,
            RewardPoints = 100,
            IsPremium = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _lessonRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Lesson>()))
            .ReturnsAsync((WahadiniCryptoQuest.Core.Entities.Lesson l) => l);

        // Act
        var result = await _lessonService.DeleteLessonAsync(lessonId);

        // Assert
        result.Should().BeTrue();
        lesson.IsDeleted.Should().BeTrue();
        lesson.DeletedAt.Should().NotBeNull();
        _lessonRepositoryMock.Verify(x => x.UpdateAsync(lesson), Times.Once);
    }

    #endregion

    #region GetByIdAsync with IncludeTasks Tests (T015, T016)

    [Fact]
    public async Task GetByIdAsync_WithIncludeTasksTrue_ReturnsLessonWithTasks()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var lesson = new WahadiniCryptoQuest.Core.Entities.Lesson
        {
            Id = lessonId,
            CourseId = courseId,
            Title = "Test Lesson",
            Description = "Test Description",
            YouTubeVideoId = "testVideoId",
            Duration = 30,
            OrderIndex = 1,
            RewardPoints = 50,
            IsPremium = false,
            CreatedAt = DateTime.UtcNow,
            Tasks = new List<LearningTask>
            {
                new() { Id = Guid.NewGuid(), Title = "Task 1", TaskType = Core.Enums.TaskType.Quiz },
                new() { Id = Guid.NewGuid(), Title = "Task 2", TaskType = Core.Enums.TaskType.Screenshot }
            }
        };

        var lessonDto = new LessonDto
        {
            Id = lessonId,
            Title = "Test Lesson",
            Tasks = new List<Core.DTOs.Task.LearningTaskDto>
            {
                new() { Id = lesson.Tasks.First().Id, Title = "Task 1" },
                new() { Id = lesson.Tasks.Last().Id, Title = "Task 2" }
            }
        };

        _lessonRepositoryMock
            .Setup(x => x.GetWithTasksAsync(lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lesson);

        _mapperMock
            .Setup(x => x.Map<LessonDto>(lesson))
            .Returns(lessonDto);

        // Act
        var result = await _lessonService.GetLessonByIdAsync(lessonId, includeTasks: true);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(lessonDto);
        result.Tasks.Should().HaveCount(2);
        result.Tasks.First().Title.Should().Be("Task 1");
        _lessonRepositoryMock.Verify(x => x.GetWithTasksAsync(lessonId, It.IsAny<CancellationToken>()), Times.Once);
        _lessonRepositoryMock.Verify(x => x.GetByIdAsync(lessonId), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WithIncludeTasksFalse_ReturnsLessonWithoutTasks()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var lesson = new WahadiniCryptoQuest.Core.Entities.Lesson
        {
            Id = lessonId,
            CourseId = courseId,
            Title = "Test Lesson",
            Description = "Test Description",
            YouTubeVideoId = "testVideoId",
            Duration = 30,
            OrderIndex = 1,
            RewardPoints = 50,
            IsPremium = false,
            CreatedAt = DateTime.UtcNow
        };

        var lessonDto = new LessonDto
        {
            Id = lessonId,
            Title = "Test Lesson",
            Tasks = null
        };

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _mapperMock
            .Setup(x => x.Map<LessonDto>(lesson))
            .Returns(lessonDto);

        // Act
        var result = await _lessonService.GetLessonByIdAsync(lessonId, includeTasks: false);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(lessonDto);
        result.Tasks.Should().BeNull();
        _lessonRepositoryMock.Verify(x => x.GetByIdAsync(lessonId), Times.Once);
        _lessonRepositoryMock.Verify(x => x.GetWithTasksAsync(lessonId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_DefaultIncludeTasks_ReturnsLessonWithoutTasks()
    {
        // Arrange - Test default behavior (includeTasks not specified)
        var lessonId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var lesson = new WahadiniCryptoQuest.Core.Entities.Lesson
        {
            Id = lessonId,
            CourseId = courseId,
            Title = "Test Lesson",
            Description = "Test Description",
            YouTubeVideoId = "testVideoId",
            Duration = 30,
            OrderIndex = 1,
            RewardPoints = 50,
            IsPremium = false,
            CreatedAt = DateTime.UtcNow
        };

        var lessonDto = new LessonDto
        {
            Id = lessonId,
            Title = "Test Lesson"
        };

        _lessonRepositoryMock
            .Setup(x => x.GetByIdAsync(lessonId))
            .ReturnsAsync(lesson);

        _mapperMock
            .Setup(x => x.Map<LessonDto>(lesson))
            .Returns(lessonDto);

        // Act - Call without includeTasks parameter
        var result = await _lessonService.GetLessonByIdAsync(lessonId);

        // Assert - Should use non-tasks repository method by default
        result.Should().NotBeNull();
        _lessonRepositoryMock.Verify(x => x.GetByIdAsync(lessonId), Times.Once);
        _lessonRepositoryMock.Verify(x => x.GetWithTasksAsync(lessonId, It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private static WahadiniCryptoQuest.Core.Entities.Lesson CreateTestLesson(
        Guid courseId,
        string title,
        int orderIndex)
    {
        return new WahadiniCryptoQuest.Core.Entities.Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            Description = "Test Description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 15,
            OrderIndex = orderIndex,
            RewardPoints = 100,
            IsPremium = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
