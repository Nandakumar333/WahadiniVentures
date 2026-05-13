using FluentValidation;
using WahadiniCryptoQuest.Service.Commands.Auth;

namespace WahadiniCryptoQuest.Service.Validators.Auth;

/// <summary>
/// Validator for user login requests
/// </summary>
public class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Please enter a valid email address")
            .Must(email => !string.IsNullOrWhiteSpace(email) && !email.Contains(" "))
            .WithMessage("Please enter a valid email address")
            .MaximumLength(254)
            .WithMessage("Email must not exceed 254 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(1)
            .WithMessage("Password is required");
    }
}