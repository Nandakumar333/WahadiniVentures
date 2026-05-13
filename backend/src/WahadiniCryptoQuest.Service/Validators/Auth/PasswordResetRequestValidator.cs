using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Auth;

namespace WahadiniCryptoQuest.Service.Validators.Auth;

/// <summary>
/// FluentValidation validator for PasswordResetRequest
/// Validates forgot password / reset password initiation requests
/// </summary>
public class PasswordResetRequestValidator : AbstractValidator<PasswordResetRequest>
{
    public PasswordResetRequestValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256)
            .WithMessage("Email cannot exceed 256 characters");

        // Optional client info validation
        RuleFor(x => x.ClientInfo)
            .MaximumLength(500)
            .WithMessage("Client info cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ClientInfo));

        // Optional IP address validation
        RuleFor(x => x.IpAddress)
            .MaximumLength(45)
            .WithMessage("IP address cannot exceed 45 characters")
            .When(x => !string.IsNullOrEmpty(x.IpAddress));
    }
}
