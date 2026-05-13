using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Progress;

namespace WahadiniCryptoQuest.API.Validators.Progress;

/// <summary>
/// Validator for UpdateProgressDto to ensure progress updates are valid
/// </summary>
public class UpdateProgressValidator : AbstractValidator<UpdateProgressDto>
{
    public UpdateProgressValidator()
    {
        RuleFor(x => x.WatchPosition)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Watch position must be greater than or equal to 0 seconds.")
            .WithErrorCode("INVALID_WATCH_POSITION");

        // Note: Video duration validation is done in the service layer
        // since we need to fetch the lesson entity to get the duration
    }
}
