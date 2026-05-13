using FluentValidation.TestHelper;
using Moq;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using Xunit;
using WahadiniCryptoQuest.API.Validators.Course;

namespace WahadiniCryptoQuest.API.Tests.Validators.Course;

/// <summary>
/// Tests for CreateCourseValidator with boundary conditions (T174)
/// </summary>
public class CreateCourseValidatorTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CreateCourseValidator _validator;
    private readonly Guid _validCategoryId = Guid.NewGuid();

    public CreateCourseValidatorTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _validator = new CreateCourseValidator(_categoryRepositoryMock.Object);

        // Setup: Valid category exists
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(_validCategoryId))
            .ReturnsAsync(new Category
            {
                Id = _validCategoryId,
                Name = "Test Category",
                DisplayOrder = 1
            });
    }

    #region Title Validation Tests

    [Fact]
    public async Task Title_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto() with { Title = string.Empty };

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

    #region CategoryId Validation Tests

    [Fact]
    public async Task CategoryId_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { CategoryId = Guid.Empty };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category is required");
    }

    [Fact]
    public async Task CategoryId_WhenDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        var nonExistentCategoryId = Guid.NewGuid();
        dto = dto with { CategoryId = nonExistentCategoryId };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(nonExistentCategoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category does not exist");
    }

    [Fact]
    public async Task CategoryId_WhenExists_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    #endregion

    #region ThumbnailUrl Validation Tests

    [Fact]
    public async Task ThumbnailUrl_With500Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { ThumbnailUrl = "https://example.com/" + new string('a', 480) }; // Total 500 chars (20 + 480)

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ThumbnailUrl);
    }

    [Fact]
    public async Task ThumbnailUrl_With501Characters_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { ThumbnailUrl = "https://example.com/" + new string('a', 481) }; // Total 501 chars (20 + 481)

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ThumbnailUrl)
            .WithErrorMessage("Thumbnail URL cannot exceed 500 characters");
    }

    [Theory]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("data:text/html,<script>alert('XSS')</script>")]
    public async Task ThumbnailUrl_WithUnsafeUrl_ShouldHaveValidationError(string unsafeUrl)
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { ThumbnailUrl = unsafeUrl };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ThumbnailUrl)
            .WithErrorMessage("Thumbnail URL must be a valid and safe URL (XSS prevention)");
    }

    [Fact]
    public async Task ThumbnailUrl_WhenNull_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { ThumbnailUrl = null };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ThumbnailUrl);
    }

    #endregion

    #region EstimatedDuration Validation Tests

    [Fact]
    public async Task EstimatedDuration_WhenZero_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { EstimatedDuration = 0 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EstimatedDuration)
            .WithErrorMessage("Estimated duration must be greater than 0");
    }

    [Fact]
    public async Task EstimatedDuration_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { EstimatedDuration = -1 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EstimatedDuration)
            .WithErrorMessage("Estimated duration must be greater than 0");
    }

    [Fact]
    public async Task EstimatedDuration_WhenPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = CreateValidDto();
        dto = dto with { EstimatedDuration = 3600 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EstimatedDuration);
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
        dto = dto with { RewardPoints = 100 };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RewardPoints);
    }

    #endregion

    #region Valid Object Tests

    [Fact]
    public async Task ValidCourse_ShouldNotHaveAnyValidationErrors()
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

    private CreateCourseDto CreateValidDto()
    {
        return new CreateCourseDto
        {
            Title = "Introduction to Bitcoin",
            Description = "Learn the fundamentals of Bitcoin cryptocurrency",
            CategoryId = _validCategoryId,
            ThumbnailUrl = "https://example.com/thumbnail.jpg",
            DifficultyLevel = DifficultyLevel.Beginner,
            EstimatedDuration = 3600,
            IsPremium = false,
            RewardPoints = 100
        };
    }

    #endregion
}
