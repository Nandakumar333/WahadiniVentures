using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.API.Utils;

namespace WahadiniCryptoQuest.API.Validators.Course;

/// <summary>
/// Validator for course update requests
/// Includes XSS prevention validation (T180)
/// </summary>
public class UpdateCourseValidator : AbstractValidator<UpdateCourseDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICourseRepository _courseRepository;

    public UpdateCourseValidator(ICategoryRepository categoryRepository, ICourseRepository courseRepository)
    {
        _categoryRepository = categoryRepository;
        _courseRepository = courseRepository;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Course ID is required")
            .MustAsync(CourseExists).WithMessage("Course does not exist");

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

        RuleFor(x => x.ThumbnailUrl)
            .MaximumLength(500).WithMessage("Thumbnail URL cannot exceed 500 characters")
            .Must(url => InputSanitizer.IsSafeUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.ThumbnailUrl))
            .WithMessage("Thumbnail URL must be a valid and safe URL (XSS prevention)");

        RuleFor(x => x.EstimatedDuration)
            .GreaterThan(0).WithMessage("Estimated duration must be greater than 0");

        RuleFor(x => x.RewardPoints)
            .GreaterThanOrEqualTo(0).WithMessage("Reward points cannot be negative");
    }

    private async Task<bool> CourseExists(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        return course != null;
    }

    private async Task<bool> CategoryExists(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        return category != null;
    }
}
