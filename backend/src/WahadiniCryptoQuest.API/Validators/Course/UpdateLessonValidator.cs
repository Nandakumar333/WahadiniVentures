using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.API.Validators.Course;

/// <summary>
/// Validator for lesson update requests
/// </summary>
public class UpdateLessonValidator : AbstractValidator<UpdateLessonDto>
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonRepository _lessonRepository;

    public UpdateLessonValidator(ICourseRepository courseRepository, ILessonRepository lessonRepository)
    {
        _courseRepository = courseRepository;
        _lessonRepository = lessonRepository;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Lesson ID is required")
            .MustAsync(LessonExists).WithMessage("Lesson does not exist");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required")
            .MustAsync(CourseExists).WithMessage("Course does not exist");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

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

    private async Task<bool> LessonExists(Guid lessonId, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        return lesson != null;
    }

    private async Task<bool> CourseExists(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        return course != null;
    }
}
