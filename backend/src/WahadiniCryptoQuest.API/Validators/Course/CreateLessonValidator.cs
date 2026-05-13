using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.API.Utils;

namespace WahadiniCryptoQuest.API.Validators.Course;

/// <summary>
/// Validator for lesson creation requests
/// Includes XSS prevention validation (T180)
/// </summary>
public class CreateLessonValidator : AbstractValidator<CreateLessonDto>
{
    private readonly ICourseRepository _courseRepository;

    public CreateLessonValidator(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;

        RuleFor(x => x.CourseId)
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

        RuleFor(x => x.YouTubeVideoId)
            .NotEmpty().WithMessage("YouTube Video ID is required")
            .Length(11).WithMessage("YouTube Video ID must be exactly 11 characters")
            .Matches(@"^[a-zA-Z0-9_-]{11}$").WithMessage("YouTube Video ID must contain only alphanumeric characters, hyphens, and underscores");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Duration must be greater than 0");

        RuleFor(x => x.OrderIndex)
            .GreaterThan(0).WithMessage("Order index must be greater than 0");

        RuleFor(x => x.RewardPoints)
            .GreaterThanOrEqualTo(0).WithMessage("Reward points cannot be negative");
    }

    private async Task<bool> CourseExists(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        return course != null;
    }
}
