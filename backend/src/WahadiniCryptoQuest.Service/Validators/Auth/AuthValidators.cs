using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Auth;

namespace WahadiniCryptoQuest.Service.Validators.Auth;

/// <summary>
/// FluentValidation validator for EmailConfirmationDto
/// Validates email confirmation token requests
/// </summary>
public class EmailConfirmationDtoValidator : AbstractValidator<EmailConfirmationDto>
{
    public EmailConfirmationDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Verification token is required")
            .MaximumLength(500)
            .WithMessage("Token cannot exceed 500 characters");
    }
}

/// <summary>
/// FluentValidation validator for ResendEmailConfirmationDto
/// Validates resend email confirmation requests
/// </summary>
public class ResendEmailConfirmationDtoValidator : AbstractValidator<ResendEmailConfirmationDto>
{
    public ResendEmailConfirmationDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Please enter a valid email address")
            .MaximumLength(320)
            .WithMessage("Email address cannot exceed 320 characters");
    }
}