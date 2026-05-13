using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class CategoryRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CategoryRepository _repository;
    private readonly CourseRepository _courseRepository;

    public CategoryRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CategoryRepository(_context);
        _courseRepository = new CourseRepository(_context);
    }

    [Fact]
    public async Task GetActiveCategoriesOrderedAsync_ShouldReturnActiveCategoriesOrdered()
    {
        // Arrange
        var c1 = new Category { Name = "B", DisplayOrder = 2, IsActive = true };
        var c2 = new Category { Name = "A", DisplayOrder = 1, IsActive = true };
        var c3 = new Category { Name = "C", DisplayOrder = 3, IsActive = false };

        await _repository.AddAsync(c1);
        await _repository.AddAsync(c2);
        await _repository.AddAsync(c3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveCategoriesOrderedAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("A");
        result.Last().Name.Should().Be("B");
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnCategory()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Test Category");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetWithCourseCountAsync_ShouldReturnCategoriesWithCount()
    {
        // Arrange
        var category = new Category { Name = "Test", IsActive = true };
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        var course1 = new Course
        {
            Title = "C1",
            Description = "D1",
            CategoryId = category.Id,
            DifficultyLevel = DifficultyLevel.Beginner,
            EstimatedDuration = 10,
            RewardPoints = 10,
            IsPremium = false,
            ThumbnailUrl = "url",
            IsPublished = false
        };

        var course2 = new Course
        {
            Title = "C2",
            Description = "D2",
            CategoryId = category.Id,
            DifficultyLevel = DifficultyLevel.Beginner,
            EstimatedDuration = 10,
            RewardPoints = 10,
            IsPremium = false,
            ThumbnailUrl = "url",
            IsPublished = false
        };
        // course2 is not published - we'll add a lesson to course1 and publish it

        await _courseRepository.AddAsync(course1);
        await _courseRepository.AddAsync(course2);
        await _context.SaveChangesAsync();

        var lesson1 = new Lesson
        {
            CourseId = course1.Id,
            Title = "Lesson 1",
            Description = "Description",
            YouTubeVideoId = "video123",
            Duration = 10,
            OrderIndex = 1,
            RewardPoints = 10
        };

        _context.Lessons.Add(lesson1);
        await _context.SaveChangesAsync();

        course1.Publish();
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithCourseCountAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().CourseCount.Should().Be(1); // Only published courses count
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
