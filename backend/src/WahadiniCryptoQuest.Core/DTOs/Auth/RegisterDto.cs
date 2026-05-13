using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Data Transfer Object for user registration requests
/// Contains validation rules and input constraints
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// User's first name
    /// Required, 2-50 characters
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// Required, 2-50 characters
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// Must be valid email format, max 320 characters (RFC 5321)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(320, ErrorMessage = "Email address cannot exceed 320 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password
    /// Must meet strong password requirements
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "Password must contain at least 8 characters with at least one lowercase letter, one uppercase letter, one number, and one special character")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation
    /// Must match the password field
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Terms of service acceptance
    /// Must be true for registration to proceed
    /// </summary>
    [Required(ErrorMessage = "You must accept the terms of service")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms of service")]
    public bool AcceptTerms { get; set; } = false;

    /// <summary>
    /// Marketing emails opt-in (optional)
    /// User can choose to receive marketing communications
    /// </summary>
    public bool AcceptMarketing { get; set; } = false;
}