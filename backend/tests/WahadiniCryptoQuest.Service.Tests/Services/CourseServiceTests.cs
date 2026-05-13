using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Exceptions;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Tests.Services;

/// <summary>
/// Unit tests for CourseService
/// Tests critical business logic for course management
/// </summary>
public class CourseServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IRepository<UserCourseEnrollment>> _enrollmentRepositoryMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<WahadiniCryptoQuest.Service.Course.CourseService>> _loggerMock;
    private readonly ICourseService _courseService;

    public CourseServiceTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _enrollmentRepositoryMock = new Mock<IRepository<UserCourseEnrollment>>();
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<Course.CourseService>>();

        // Setup UnitOfWork SaveChangesAsync to return 1 (indicating 1 entity saved)
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _courseService = new Course.CourseService(
            _courseRepositoryMock.Object,
            _enrollmentRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    #region EnrollUserAsync Tests

    [Fact]
    public async Task EnrollUserAsync_ValidRequest_EnrollsUserSuccessfully()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            Description = "Description",
            CategoryId = Guid.NewGuid(),
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            CreatedByUserId = adminId,
            IsPublished = true,
            Lessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>
            {
                new WahadiniCryptoQuest.Core.Entities.Lesson { Id = Guid.NewGuid() }
            }
        };

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolledAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _enrollmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserCourseEnrollment>()))
            .ReturnsAsync((UserCourseEnrollment e) => e);

        // Act
        var result = await _courseService.EnrollUserAsync(courseId, userId);

        // Assert
        result.Should().BeTrue();
        _enrollmentRepositoryMock.Verify(
            x => x.AddAsync(It.Is<UserCourseEnrollment>(e =>
                e.CourseId == courseId &&
                e.UserId == userId &&
                e.EnrolledAt != default)),
            Times.Once);
    }

    [Fact]
    public async Task EnrollUserAsync_DuplicateEnrollment_ReturnsFalse()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolledAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _courseService.EnrollUserAsync(courseId, userId);

        // Assert
        result.Should().BeFalse();
        _enrollmentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserCourseEnrollment>()),
            Times.Never);
    }

    [Fact]
    public async Task EnrollUserAsync_PremiumCourseWithFreeUser_ThrowsPremiumAccessDeniedException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var premiumCourse = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Premium Course",
            Description = "Description",
            CategoryId = Guid.NewGuid(),
            DifficultyLevel = DifficultyLevel.Advanced,
            IsPremium = true,
            CreatedByUserId = adminId,
            IsPublished = true,
            Lessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>
            {
                new WahadiniCryptoQuest.Core.Entities.Lesson { Id = Guid.NewGuid() }
            }
        };

        var freeUser = User.Create("test@example.com", "hashedpassword", "John", "Doe");

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolledAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(premiumCourse);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(freeUser);

        // Act & Assert
        await Assert.ThrowsAsync<PremiumAccessDeniedException>(
            async () => await _courseService.EnrollUserAsync(courseId, userId));

        _enrollmentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserCourseEnrollment>()),
            Times.Never);
    }

    [Fact]
    public async Task EnrollUserAsync_PremiumCourseWithPremiumUser_EnrollsSuccessfully()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var premiumCourse = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Premium Course",
            Description = "Description",
            CategoryId = Guid.NewGuid(),
            DifficultyLevel = DifficultyLevel.Advanced,
            IsPremium = true,
            CreatedByUserId = adminId,
            IsPublished = true,
            Lessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>
            {
                new WahadiniCryptoQuest.Core.Entities.Lesson { Id = Guid.NewGuid() }
            }
        };

        var premiumUser = User.Create("premium@example.com", "hashedpassword", "Jane", "Smith");
        premiumUser.UpgradeRole(UserRoleEnum.Premium);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolledAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(premiumCourse);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(premiumUser);

        _enrollmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserCourseEnrollment>()))
            .ReturnsAsync((UserCourseEnrollment e) => e);

        // Act
        var result = await _courseService.EnrollUserAsync(courseId, userId);

        // Assert
        result.Should().BeTrue();
        _enrollmentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserCourseEnrollment>()),
            Times.Once);
    }

    [Fact]
    public async Task EnrollUserAsync_UnpublishedCourse_ThrowsInvalidOperationException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var unpublishedCourse = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Unpublished Course",
            Description = "Description",
            CategoryId = Guid.NewGuid(),
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            CreatedByUserId = adminId,
            IsPublished = false
        };

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolledAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(unpublishedCourse);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _courseService.EnrollUserAsync(courseId, userId));
    }

    #endregion

    #region PublishCourseAsync Tests

    [Fact]
    public async Task PublishCourseAsync_ValidCourse_PublishesSuccessfully()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            Description = "Description",
            CategoryId = Guid.NewGuid(),
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            CreatedByUserId = adminId,
            IsPublished = false,
            Lessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>
            {
                new WahadiniCryptoQuest.Core.Entities.Lesson { Id = Guid.NewGuid() }
            }
        };

        _courseRepositoryMock
            .Setup(x => x.GetWithLessonsAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Course>()))
            .ReturnsAsync((WahadiniCryptoQuest.Core.Entities.Course c) => c);

        // Act
        var result = await _courseService.PublishCourseAsync(courseId);

        // Assert
        result.Should().BeTrue();
        course.IsPublished.Should().BeTrue();
        _courseRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<WahadiniCryptoQuest.Core.Entities.Course>(c => c.Id == courseId && c.IsPublished)),
            Times.Once);
    }

    [Fact]
    public async Task PublishCourseAsync_NonExistentCourse_ReturnsFalse()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _courseRepositoryMock
            .Setup(x => x.GetWithLessonsAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WahadiniCryptoQuest.Core.Entities.Course?)null);

        // Act
        var result = await _courseService.PublishCourseAsync(courseId);

        // Assert
        result.Should().BeFalse();
        _courseRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Course>()),
            Times.Never);
    }

    [Fact]
    public async Task PublishCourseAsync_CourseWithZeroLessons_ThrowsInvalidOperationException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var courseWithNoLessons = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Empty Course",
            Description = "Description",
            CategoryId = Guid.NewGuid(),
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            CreatedByUserId = adminId,
            IsPublished = false,
            Lessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>() // Empty list
        };

        _courseRepositoryMock
            .Setup(x => x.GetWithLessonsAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseWithNoLessons);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _courseService.PublishCourseAsync(courseId));

        _courseRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Course>()),
            Times.Never);
    }

    [Fact]
    public async Task PublishCourseAsync_CourseWithOneLesson_SetsIsPublishedTrue()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var courseWithOneLesson = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Valid Course",
            Description = "Description",
            CategoryId = Guid.NewGuid(),
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            CreatedByUserId = adminId,
            IsPublished = false,
            Lessons = new List<WahadiniCryptoQuest.Core.Entities.Lesson>
            {
                new WahadiniCryptoQuest.Core.Entities.Lesson { Id = Guid.NewGuid() }
            }
        };

        _courseRepositoryMock
            .Setup(x => x.GetWithLessonsAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseWithOneLesson);

        _courseRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<WahadiniCryptoQuest.Core.Entities.Course>()))
            .ReturnsAsync((WahadiniCryptoQuest.Core.Entities.Course c) => c);

        // Act
        var result = await _courseService.PublishCourseAsync(courseId);

        // Assert
        result.Should().BeTrue();
        courseWithOneLesson.IsPublished.Should().BeTrue();
    }

    #endregion

    #region GetCoursesAsync Tests (T045)

    [Fact]
    public async Task GetCoursesAsync_WithCategoryFilter_ReturnsFilteredResults()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = Guid.NewGuid(),
            Title = "Beginner Airdrop Course",
            Description = "Learn airdrops",
            CategoryId = categoryId,
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            IsPublished = true,
            CreatedByUserId = Guid.NewGuid()
        };

        var pagedResult = new PagedResult<WahadiniCryptoQuest.Core.Entities.Course>(
            new List<WahadiniCryptoQuest.Core.Entities.Course> { course }, 1, 1, 10);

        _courseRepositoryMock
            .Setup(x => x.GetByCategoryAsync(categoryId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var courseDto = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            CategoryName = "Airdrop",
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false
        };

        _mapperMock.Setup(x => x.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<WahadiniCryptoQuest.Core.Entities.Course>>()))
            .Returns(new List<CourseDto> { courseDto });

        // Act
        var result = await _courseService.GetCoursesAsync(categoryId, null, null, null, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items.First().DifficultyLevel.Should().Be(DifficultyLevel.Beginner);
    }

    [Fact]
    public async Task GetCoursesAsync_WithSearchTerm_ReturnsMatchingCourses()
    {
        // Arrange
        var courses = new List<WahadiniCryptoQuest.Core.Entities.Course>
        {
            new WahadiniCryptoQuest.Core.Entities.Course
            {
                Id = Guid.NewGuid(),
                Title = "Bitcoin Basics",
                Description = "Learn Bitcoin fundamentals",
                CategoryId = Guid.NewGuid(),
                DifficultyLevel = DifficultyLevel.Beginner,
                IsPremium = false,
                IsPublished = true,
                CreatedByUserId = Guid.NewGuid()
            }
        };

        var pagedResult = new PagedResult<WahadiniCryptoQuest.Core.Entities.Course>(courses, 1, 1, 10);

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesAsync("Bitcoin", null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var courseDtos = courses.Select(c => new CourseDto { Id = c.Id, Title = c.Title }).ToList();

        _mapperMock.Setup(x => x.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<WahadiniCryptoQuest.Core.Entities.Course>>()))
            .Returns(courseDtos);

        // Act
        var result = await _courseService.GetCoursesAsync(null, null, null, "Bitcoin", 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCoursesAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var courses = Enumerable.Range(1, 2).Select(i =>
            new WahadiniCryptoQuest.Core.Entities.Course
            {
                Id = Guid.NewGuid(),
                Title = $"Course {i + 2}",
                Description = "Description",
                CategoryId = Guid.NewGuid(),
                DifficultyLevel = DifficultyLevel.Beginner,
                IsPremium = false,
                IsPublished = true,
                CreatedByUserId = Guid.NewGuid()
            }).ToList();

        var pagedResult = new PagedResult<WahadiniCryptoQuest.Core.Entities.Course>(courses, 5, 2, 2);

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesAsync("", null, null, 2, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var courseDtos = courses.Select(c => new CourseDto { Id = c.Id, Title = c.Title }).ToList();

        _mapperMock.Setup(x => x.Map<IEnumerable<CourseDto>>(It.IsAny<IEnumerable<WahadiniCryptoQuest.Core.Entities.Course>>()))
            .Returns(courseDtos);

        // Act
        var result = await _courseService.GetCoursesAsync(null, null, null, null, 2, 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
    }

    #endregion

    #region GetUserCoursesAsync Tests (T149)

    [Fact]
    public async Task GetUserCoursesAsync_CalculatesProgressAccurately()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var category = new Category { Id = categoryId, Name = "Crypto Basics" };

        var enrollment = new UserCourseEnrollment
        {
            UserId = userId,
            CourseId = courseId,
            CompletionPercentage = 30, // 30% complete
            IsCompleted = false,
            LastAccessedAt = DateTime.UtcNow
        };

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Test Course",
            Description = "Description",
            CategoryId = categoryId,
            Category = category,
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            IsPublished = true,
            EstimatedDuration = 60,
            RewardPoints = 100,
            CreatedByUserId = Guid.NewGuid()
        };

        _enrollmentRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserCourseEnrollment, bool>>>()))
            .ReturnsAsync(new List<UserCourseEnrollment> { enrollment });

        _courseRepositoryMock
            .Setup(x => x.GetWithLessonsAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Act
        var result = await _courseService.GetUserCoursesAsync(userId);

        // Assert
        var enrolledCourse = result.Should().ContainSingle().Subject;
        enrolledCourse.ProgressPercentage.Should().Be(30);
        enrolledCourse.CompletionStatus.Should().Be(CompletionStatus.InProgress);
    }

    [Fact]
    public async Task GetUserCoursesAsync_CompletedCourse_HasCorrectStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var category = new Category { Id = categoryId, Name = "Advanced Crypto" };

        var enrollment = new UserCourseEnrollment
        {
            UserId = userId,
            CourseId = courseId,
            CompletionPercentage = 100, // Fully complete
            IsCompleted = true,
            LastAccessedAt = DateTime.UtcNow.AddDays(-1)
        };

        var course = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId,
            Title = "Completed Course",
            Description = "Description",
            CategoryId = categoryId,
            Category = category,
            DifficultyLevel = DifficultyLevel.Advanced,
            IsPremium = true,
            IsPublished = true,
            EstimatedDuration = 120,
            RewardPoints = 500,
            CreatedByUserId = Guid.NewGuid()
        };

        _enrollmentRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserCourseEnrollment, bool>>>()))
            .ReturnsAsync(new List<UserCourseEnrollment> { enrollment });

        _courseRepositoryMock
            .Setup(x => x.GetWithLessonsAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Act
        var result = await _courseService.GetUserCoursesAsync(userId);

        // Assert
        var enrolledCourse = result.Should().ContainSingle().Subject;
        enrolledCourse.ProgressPercentage.Should().Be(100);
        enrolledCourse.CompletionStatus.Should().Be(CompletionStatus.Completed);
    }

    [Fact]
    public async Task GetUserCoursesAsync_FilterByStatus_ReturnsOnlyMatching()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var category = new Category { Id = categoryId, Name = "Crypto Trading" };

        var enrollments = new List<UserCourseEnrollment>
        {
            new UserCourseEnrollment
            {
                UserId = userId,
                CourseId = courseId1,
                CompletionPercentage = 100,
                IsCompleted = true,
                LastAccessedAt = DateTime.UtcNow
            },
            new UserCourseEnrollment
            {
                UserId = userId,
                CourseId = courseId2,
                CompletionPercentage = 50,
                IsCompleted = false,
                LastAccessedAt = DateTime.UtcNow
            }
        };

        var course1 = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId1,
            Title = "Completed Course",
            Description = "Description",
            CategoryId = categoryId,
            Category = category,
            DifficultyLevel = DifficultyLevel.Beginner,
            IsPremium = false,
            IsPublished = true,
            EstimatedDuration = 60,
            RewardPoints = 100,
            CreatedByUserId = Guid.NewGuid()
        };

        var course2 = new WahadiniCryptoQuest.Core.Entities.Course
        {
            Id = courseId2,
            Title = "In Progress Course",
            Description = "Description",
            CategoryId = categoryId,
            Category = category,
            DifficultyLevel = DifficultyLevel.Intermediate,
            IsPremium = false,
            IsPublished = true,
            EstimatedDuration = 90,
            RewardPoints = 200,
            CreatedByUserId = Guid.NewGuid()
        };

        _enrollmentRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserCourseEnrollment, bool>>>()))
            .ReturnsAsync(enrollments);

        _courseRepositoryMock
            .Setup(x => x.GetWithLessonsAsync(courseId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course1);

        _courseRepositoryMock
            .Setup(x => x.GetWithLessonsAsync(courseId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course2);

        // Act
        var result = await _courseService.GetUserCoursesAsync(userId, CompletionStatus.Completed);

        // Assert
        result.Should().ContainSingle();
        result.First().CompletionStatus.Should().Be(CompletionStatus.Completed);
        result.First().ProgressPercentage.Should().Be(100);
    }

    #endregion
}
