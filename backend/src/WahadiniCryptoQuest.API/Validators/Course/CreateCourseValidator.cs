using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.API.Utils;

namespace WahadiniCryptoQuest.API.Validators.Course;

/// <summary>
/// Validator for course creation requests
/// Includes XSS prevention validation (T180)
/// </summary>
public class CreateCourseValidator : AbstractValidator<CreateCourseDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCourseValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .Must(title => !InputSanitizer.ContainsDangerousContent(title))
            .WithMessage("Title contains potentially dangerous content (XSS prevention)");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .Must(desc => !InputSanitizer.ContainsDangerousContent(desc))
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description contains potentially dangerous content (XSS prevention)");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required")
            .MustAsync(CategoryExists).WithMessage("Category does not exist");

        // ThumbnailUrl validation - MaximumLength applies even to null/empty (FluentValidation ignores null for length validators)
        RuleFor(x => x.ThumbnailUrl)
            .MaximumLength(500).WithMessage("Thumbnail URL cannot exceed 500 characters");

        // Only validate URL safety when a value is provided
        RuleFor(x => x.ThumbnailUrl)
            .Must(url => InputSanitizer.IsSafeUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.ThumbnailUrl))
            .WithMessage("Thumbnail URL must be a valid and safe URL (XSS prevention)");

        RuleFor(x => x.EstimatedDuration)
            .GreaterThan(0).WithMessage("Estimated duration must be greater than 0");

        RuleFor(x => x.RewardPoints)
            .GreaterThanOrEqualTo(0).WithMessage("Reward points cannot be negative");
    }

    private async Task<bool> CategoryExists(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        return category != null;
    }
}
