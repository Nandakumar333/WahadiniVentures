using FluentValidation;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Validators.Admin;

/// <summary>
/// Validator for unban user command
/// T069: US3 - User Account Management
/// </summary>
public class UnbanUserValidator : AbstractValidator<UnbanUserCommand>
{
    public UnbanUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("Admin user ID is required");
    }
}
