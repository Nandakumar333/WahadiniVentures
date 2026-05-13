using FluentValidation.TestHelper;
using FluentAssertions;
using Moq;
using WahadiniCryptoQuest.API.Validators.Course;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Validators.Course;

/// <summary>
/// Tests for CreateLessonValidator with boundary conditions and YouTube ID format validation (T174)
/// </summary>
public class CreateLessonValidatorTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly CreateLessonValidator _validator;
    private readonly Guid _validCourseId = Guid.NewGuid();

    public CreateLessonValidatorTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _validator = new CreateLessonValidator(_courseRepositoryMock.Object);

        // Setup: Valid course exists
        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(_validCourseId))
            .ReturnsAsync(new Core.Entities.Course
            {
                Id = _validCourseId,
                Title = "Test Course",
                CategoryId = Guid.NewGuid()
            });
    }

    #region CourseId Validation Tests

    [Fact]
    public async Task CourseId_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto() with { CourseId = Guid.Empty };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CourseId)
            .WithErrorMessage("Course ID is required");
    }

    [Fact]
    public async Task CourseId_WhenDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        var nonExistentCourseId = Guid.NewGuid();
        dto = dto with { CourseId = nonExistentCourseId };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(nonExistentCourseId))
            .ReturnsAsync((Core.Entities.Course?)null);

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CourseId)
            .WithErrorMessage("Course does not exist");
    }

    [Fact]
    public async Task CourseId_WhenExists_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CourseId);
    }

    #endregion

    #region Title Validation Tests

    [Fact]
    public async Task Title_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Title = string.Empty };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public async Task Title_With199Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Title = new string('A', 199) }; // Boundary: Just below limit

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Title_With200Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Title = new string('A', 200) }; // Boundary: Exactly at limit

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Title_With201Characters_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Title = new string('A', 201) }; // Boundary: Just above limit

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("Title with <img src=x onerror=alert(1)>")]
    [InlineData("javascript:alert('XSS')")]
    public async Task Title_WithDangerousContent_ShouldHaveValidationError(string dangerousTitle)
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Title = dangerousTitle };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title contains potentially dangerous content (XSS prevention)");
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Description_With2000Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Description = new string('A', 2000) }; // Boundary: Exactly at limit

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Description_With2001Characters_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Description = new string('A', 2001) }; // Boundary: Just above limit

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 2000 characters");
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("Description with <img src=x onerror=alert(1)>")]
    public async Task Description_WithDangerousContent_ShouldHaveValidationError(string dangerousDescription)
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Description = dangerousDescription };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description contains potentially dangerous content (XSS prevention)");
    }

    [Fact]
    public async Task Description_WhenNull_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Description = null! };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    #endregion

    #region YouTube Video ID Validation Tests

    [Fact]
    public async Task YouTubeVideoId_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { YouTubeVideoId = string.Empty };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.YouTubeVideoId)
            .WithErrorMessage("YouTube Video ID is required");
    }

    [Theory]
    [InlineData("dQw4w9WgXcQ")] // Valid: 11 chars, alphanumeric
    [InlineData("0123456789a")] // Valid: 11 chars, numbers and letter
    [InlineData("abc-def_ghi")] // Valid: 11 chars with hyphen and underscore
    [InlineData("_-_-_-_-_-_")] // Valid: 11 chars all special allowed chars
    public async Task YouTubeVideoId_WithValid11CharFormat_ShouldNotHaveValidationError(string validVideoId)
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { YouTubeVideoId = validVideoId };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.YouTubeVideoId);
    }

    [Theory]
    [InlineData("dQw4w9WgXc")] // Invalid: 10 chars (too short)
    [InlineData("dQw4w9WgXcQ1")] // Invalid: 12 chars (too long)
    [InlineData("")] // Invalid: Empty
    [InlineData("   ")] // Invalid: Whitespace only
    public async Task YouTubeVideoId_WithInvalidLength_ShouldHaveValidationError(string invalidVideoId)
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { YouTubeVideoId = invalidVideoId };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert - Should have error for either empty or length
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateLessonDto.YouTubeVideoId));
    }

    [Theory]
    [InlineData("dQw4w9WgXc!")] // Invalid: Contains exclamation mark
    [InlineData("dQw4w9WgXc@")] // Invalid: Contains @ symbol
    [InlineData("dQw4w9WgXc#")] // Invalid: Contains # symbol
    [InlineData("dQw4w9WgXc$")] // Invalid: Contains $ symbol
    [InlineData("dQw4w9WgXc%")] // Invalid: Contains % symbol
    [InlineData("dQw4w9 gXcQ")] // Invalid: Contains space
    public async Task YouTubeVideoId_WithInvalidCharacters_ShouldHaveValidationError(string invalidVideoId)
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { YouTubeVideoId = invalidVideoId };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.YouTubeVideoId)
            .WithErrorMessage("YouTube Video ID must contain only alphanumeric characters, hyphens, and underscores");
    }

    [Fact]
    public async Task YouTubeVideoId_ExactlyAtBoundary_11Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { YouTubeVideoId = "12345678901" }; // Exactly 11 chars

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.YouTubeVideoId);
    }

    #endregion

    #region Duration Validation Tests

    [Fact]
    public async Task Duration_WhenZero_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Duration = 0 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Duration)
            .WithErrorMessage("Duration must be greater than 0");
    }

    [Fact]
    public async Task Duration_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Duration = -1 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Duration)
            .WithErrorMessage("Duration must be greater than 0");
    }

    [Fact]
    public async Task Duration_WhenPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { Duration = 600 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Duration);
    }

    #endregion

    #region OrderIndex Validation Tests

    [Fact]
    public async Task OrderIndex_WhenZero_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { OrderIndex = 0 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderIndex)
            .WithErrorMessage("Order index must be greater than 0");
    }

    [Fact]
    public async Task OrderIndex_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { OrderIndex = -1 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderIndex)
            .WithErrorMessage("Order index must be greater than 0");
    }

    [Fact]
    public async Task OrderIndex_WhenPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { OrderIndex = 1 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OrderIndex);
    }

    #endregion

    #region RewardPoints Validation Tests

    [Fact]
    public async Task RewardPoints_WhenZero_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { RewardPoints = 0 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RewardPoints);
    }

    [Fact]
    public async Task RewardPoints_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { RewardPoints = -1 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RewardPoints)
            .WithErrorMessage("Reward points cannot be negative");
    }

    [Fact]
    public async Task RewardPoints_WhenPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { RewardPoints = 50 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RewardPoints);
    }

    #endregion

    #region Valid Object Tests

    [Fact]
    public async Task ValidLesson_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var dto = CreateValidDto();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Helper Methods

    private CreateLessonDto CreateValidDto()
    {
        return new CreateLessonDto
        {
            CourseId = _validCourseId,
            Title = "Introduction to Bitcoin Wallets",
            Description = "Learn how to create and manage Bitcoin wallets",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 600,
            OrderIndex = 1,
            IsPremium = false,
            RewardPoints = 50
        };
    }

    #endregion
}
