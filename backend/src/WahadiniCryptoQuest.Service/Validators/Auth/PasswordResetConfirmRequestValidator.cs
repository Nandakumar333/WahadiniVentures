using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Auth;

namespace WahadiniCryptoQuest.Service.Validators.Auth;

/// <summary>
/// FluentValidation validator for PasswordResetConfirmRequest
/// Validates password reset confirmation with new password
/// </summary>
public class PasswordResetConfirmRequestValidator : AbstractValidator<PasswordResetConfirmRequest>
{
    public PasswordResetConfirmRequestValidator()
    {
        // Token validation
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Reset token is required")
            .MaximumLength(500)
            .WithMessage("Reset token format is invalid");

        // New password validation with comprehensive strength requirements
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .MaximumLength(100)
            .WithMessage("Password cannot exceed 100 characters")
            .Must(HaveUppercaseLetter)
            .WithMessage("Password must contain at least one uppercase letter")
            .Must(HaveLowercaseLetter)
            .WithMessage("Password must contain at least one lowercase letter")
            .Must(HaveDigit)
            .WithMessage("Password must contain at least one digit")
            .Must(HaveSpecialCharacter)
            .WithMessage("Password must contain at least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)");

        // Confirm password validation
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required")
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match");

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

    /// <summary>
    /// Checks if password contains at least one uppercase letter
    /// </summary>
    private static bool HaveUppercaseLetter(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);
    }

    /// <summary>
    /// Checks if password contains at least one lowercase letter
    /// </summary>
    private static bool HaveLowercaseLetter(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsLower);
    }

    /// <summary>
    /// Checks if password contains at least one digit
    /// </summary>
    private static bool HaveDigit(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
    }

    /// <summary>
    /// Checks if password contains at least one special character
    /// </summary>
    private static bool HaveSpecialCharacter(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        var specialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        return password.Any(c => specialCharacters.Contains(c));
    }
}
