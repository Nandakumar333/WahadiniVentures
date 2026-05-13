using FluentValidation;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using System.Text.RegularExpressions;

namespace WahadiniCryptoQuest.Service.Validators.Auth;

/// <summary>
/// FluentValidation validator for RegisterDto
/// Implements comprehensive validation rules for user registration
/// </summary>
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(320)
            .WithMessage("Email cannot exceed 320 characters");

        // First name validation
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .Length(2, 50)
            .WithMessage("First name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$")
            .WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

        // Last name validation
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .Length(2, 50)
            .WithMessage("Last name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$")
            .WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");

        // Password validation with comprehensive strength requirements
        RuleFor(x => x.Password)
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
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match");

        // Terms acceptance validation
        RuleFor(x => x.AcceptTerms)
            .Equal(true)
            .WithMessage("You must accept the terms of service to register");
    }

    /// <summary>
    /// Checks if password contains at least one uppercase letter
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <returns>True if contains uppercase letter, false otherwise</returns>
    private static bool HaveUppercaseLetter(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);
    }

    /// <summary>
    /// Checks if password contains at least one lowercase letter
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <returns>True if contains lowercase letter, false otherwise</returns>
    private static bool HaveLowercaseLetter(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsLower);
    }

    /// <summary>
    /// Checks if password contains at least one digit
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <returns>True if contains digit, false otherwise</returns>
    private static bool HaveDigit(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
    }

    /// <summary>
    /// Checks if password contains at least one special character
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <returns>True if contains special character, false otherwise</returns>
    private static bool HaveSpecialCharacter(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        var specialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        return password.Any(c => specialCharacters.Contains(c));
    }
}