using FluentValidation;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Validators.Admin;

/// <summary>
/// Validator for task review command
/// T048: US2 - Task Review Workflow
/// </summary>
public class ReviewTaskValidator : AbstractValidator<ReviewTaskCommand>
{
    public ReviewTaskValidator()
    {
        RuleFor(x => x.SubmissionId)
            .NotEmpty().WithMessage("Submission ID is required");

        RuleFor(x => x.Status)
            .Must(status => status == SubmissionStatus.Approved || status == SubmissionStatus.Rejected)
            .WithMessage("Status must be either Approved or Rejected");

        RuleFor(x => x.Feedback)
            .NotEmpty()
            .When(x => x.Status == SubmissionStatus.Rejected)
            .WithMessage("Feedback is required when rejecting a submission");

        RuleFor(x => x.Feedback)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Feedback))
            .WithMessage("Feedback cannot exceed 1000 characters");

        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("Admin user ID is required");
    }
}
