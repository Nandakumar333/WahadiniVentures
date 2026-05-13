using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class UserProgressRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserProgressRepository _repository;
    private readonly UserRepository _userRepository;
    private readonly CourseRepository _courseRepository;
    private readonly LessonRepository _lessonRepository;

    public UserProgressRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserProgressRepository(_context);
        _userRepository = new UserRepository(_context);
        _courseRepository = new CourseRepository(_context);
        _lessonRepository = new LessonRepository(_context);
    }

    [Fact]
    public async Task GetByUserAndLessonAsync_ShouldReturnProgress()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var lesson = await CreateLessonAsync();

        var progress = UserProgress.Create(user.Id, lesson.Id);
        await _repository.AddAsync(progress);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserAndLessonAsync(user.Id, lesson.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.LessonId.Should().Be(lesson.Id);
    }

    [Fact]
    public async Task UpsertProgressAsync_ShouldInsertNewProgress()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var lesson = await CreateLessonAsync();

        var progress = UserProgress.Create(user.Id, lesson.Id);

        // Act
        var result = await _repository.UpsertProgressAsync(progress);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        var saved = await _repository.GetByUserAndLessonAsync(user.Id, lesson.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpsertProgressAsync_ShouldUpdateExistingProgress()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var lesson = await CreateLessonAsync();

        var progress = UserProgress.Create(user.Id, lesson.Id);
        await _repository.AddAsync(progress);
        await _context.SaveChangesAsync();

        // Create updated progress object
        var updatedProgress = UserProgress.Create(user.Id, lesson.Id);
        updatedProgress.UpdateProgress(300, 50); // 50% completion

        // Act
        var result = await _repository.UpsertProgressAsync(updatedProgress);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _repository.GetByUserAndLessonAsync(user.Id, lesson.Id);
        saved.Should().NotBeNull();
        saved!.CompletionPercentage.Should().Be(50);
        saved.VideoWatchTimeSeconds.Should().Be(300);
    }

    [Fact]
    public async Task GetUserProgressByCourseAsync_ShouldReturnAllLessonProgress()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var course = await CreateCourseAsync("Test Course");
        var lesson1 = await CreateLessonAsync(course.Id, 1);
        var lesson2 = await CreateLessonAsync(course.Id, 2);

        var p1 = UserProgress.Create(user.Id, lesson1.Id);
        var p2 = UserProgress.Create(user.Id, lesson2.Id);

        await _repository.AddAsync(p1);
        await _repository.AddAsync(p2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserProgressByCourseAsync(user.Id, course.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.LessonId == lesson1.Id);
        result.Should().Contain(p => p.LessonId == lesson2.Id);
    }

    private async Task<User> CreateUserAsync(string email)
    {
        var user = User.Create(email, "password", "First", "Last");
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Course> CreateCourseAsync(string title)
    {
        var course = new Course
        {
            Title = title,
            Description = "Description",
            CategoryId = Guid.NewGuid(),
            DifficultyLevel = DifficultyLevel.Beginner,
            EstimatedDuration = 120,
            RewardPoints = 100,
            IsPremium = false,
            ThumbnailUrl = "http://example.com/thumb.jpg"
        };

        await _courseRepository.AddAsync(course);
        await _context.SaveChangesAsync();
        return course;
    }

    private async Task<Lesson> CreateLessonAsync(Guid? courseId = null, int orderIndex = 1)
    {
        if (courseId == null)
        {
            var course = await CreateCourseAsync("Temp Course");
            courseId = course.Id;
        }

        var lesson = new Lesson
        {
            CourseId = courseId.Value,
            Title = $"Lesson {orderIndex}",
            Description = "Description",
            YouTubeVideoId = "videoId123",
            Duration = 10,
            OrderIndex = orderIndex,
            RewardPoints = 10
        };

        await _lessonRepository.AddAsync(lesson);
        await _context.SaveChangesAsync();
        return lesson;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
