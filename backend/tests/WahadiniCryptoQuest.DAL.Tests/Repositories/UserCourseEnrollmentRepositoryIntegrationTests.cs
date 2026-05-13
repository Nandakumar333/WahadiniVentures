using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

public class UserCourseEnrollmentRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserCourseEnrollmentRepository _repository;
    private readonly UserRepository _userRepository;
    private readonly CourseRepository _courseRepository;

    public UserCourseEnrollmentRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserCourseEnrollmentRepository(_context);
        _userRepository = new UserRepository(_context);
        _courseRepository = new CourseRepository(_context);
    }

    [Fact]
    public async Task EnrollUserAsync_ShouldCreateEnrollment()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var course = await CreateCourseAsync("Test Course");

        // Act
        var enrollment = await _repository.EnrollUserAsync(user.Id, course.Id);
        await _context.SaveChangesAsync();

        // Assert
        enrollment.Should().NotBeNull();
        enrollment.UserId.Should().Be(user.Id);
        enrollment.CourseId.Should().Be(course.Id);
        enrollment.EnrolledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var savedEnrollment = await _repository.GetByIdAsync(enrollment.Id);
        savedEnrollment.Should().NotBeNull();
    }

    [Fact]
    public async Task IsUserEnrolledAsync_ShouldReturnTrueIfEnrolled()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var course = await CreateCourseAsync("Test Course");

        await _repository.EnrollUserAsync(user.Id, course.Id);
        await _context.SaveChangesAsync();

        // Act
        var isEnrolled = await _repository.IsUserEnrolledAsync(user.Id, course.Id);

        // Assert
        isEnrolled.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserEnrolledAsync_ShouldReturnFalseIfNotEnrolled()
    {
        // Arrange
        var user = await CreateUserAsync("user@example.com");
        var course = await CreateCourseAsync("Test Course");

        // Act
        var isEnrolled = await _repository.IsUserEnrolledAsync(user.Id, course.Id);

        // Assert
        isEnrolled.Should().BeFalse();
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

    public void Dispose()
    {
        _context.Dispose();
    }
}
