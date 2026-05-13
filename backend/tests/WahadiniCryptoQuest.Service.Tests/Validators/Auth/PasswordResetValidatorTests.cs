using FluentValidation.TestHelper;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Service.Validators.Auth;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Validators.Auth;

/// <summary>
/// Unit tests for PasswordResetRequestValidator
/// Tests email validation for forgot password requests
/// </summary>
public class PasswordResetRequestValidatorTests
{
    private readonly PasswordResetRequestValidator _validator;

    public PasswordResetRequestValidatorTests()
    {
        _validator = new PasswordResetRequestValidator();
    }

    [Fact]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetRequest { Email = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Email_WhenInvalidFormat_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var request = new PasswordResetRequest { Email = invalidEmail };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Email_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // 262 characters
        var request = new PasswordResetRequest { Email = longEmail };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email cannot exceed 256 characters");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co.uk")]
    [InlineData("firstname.lastname@company.com")]
    public void Email_WhenValidFormat_ShouldNotHaveValidationError(string validEmail)
    {
        // Arrange
        var request = new PasswordResetRequest { Email = validEmail };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ClientInfo_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetRequest 
        { 
            Email = "user@example.com",
            ClientInfo = new string('a', 501)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientInfo)
            .WithErrorMessage("Client info cannot exceed 500 characters");
    }

    [Fact]
    public void IpAddress_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetRequest 
        { 
            Email = "user@example.com",
            IpAddress = new string('1', 46)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
            .WithErrorMessage("IP address cannot exceed 45 characters");
    }
}

/// <summary>
/// Unit tests for PasswordResetConfirmRequestValidator
/// Tests token and password validation for password reset confirmation
/// </summary>
public class PasswordResetConfirmRequestValidatorTests
{
    private readonly PasswordResetConfirmRequestValidator _validator;

    public PasswordResetConfirmRequestValidatorTests()
    {
        _validator = new PasswordResetConfirmRequestValidator();
    }

    #region Token Validation Tests

    [Fact]
    public void Token_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = string.Empty,
            NewPassword = "ValidPass123!",
            ConfirmPassword = "ValidPass123!"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Reset token is required");
    }

    [Fact]
    public void Token_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longToken = new string('a', 501);
        var request = new PasswordResetConfirmRequest 
        { 
            Token = longToken,
            NewPassword = "ValidPass123!",
            ConfirmPassword = "ValidPass123!"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Reset token format is invalid");
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public void NewPassword_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = string.Empty,
            ConfirmPassword = string.Empty
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password is required");
    }

    [Theory]
    [InlineData("Short1!")]
    [InlineData("Pass1!")]
    public void NewPassword_WhenLessThan8Characters_ShouldHaveValidationError(string shortPassword)
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = shortPassword,
            ConfirmPassword = shortPassword
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must be at least 8 characters long");
    }

    [Fact]
    public void NewPassword_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longPassword = new string('a', 101);
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = longPassword,
            ConfirmPassword = longPassword
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("password123!")]
    [InlineData("alllowercase1!")]
    public void NewPassword_WhenMissingUppercase_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one uppercase letter");
    }

    [Theory]
    [InlineData("PASSWORD123!")]
    [InlineData("ALLUPPERCASE1!")]
    public void NewPassword_WhenMissingLowercase_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one lowercase letter");
    }

    [Theory]
    [InlineData("Password!")]
    [InlineData("NoDigitsHere!")]
    public void NewPassword_WhenMissingDigit_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one digit");
    }

    [Theory]
    [InlineData("Password123")]
    [InlineData("NoSpecialChars123")]
    public void NewPassword_WhenMissingSpecialCharacter_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = password,
            ConfirmPassword = password
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)");
    }

    [Theory]
    [InlineData("ValidPass123!")]
    [InlineData("MySecure@Pass1")]
    [InlineData("Complex#Pass99")]
    public void NewPassword_WhenMeetsAllRequirements_ShouldNotHaveValidationError(string validPassword)
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = validPassword,
            ConfirmPassword = validPassword
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    #endregion

    #region Confirm Password Tests

    [Fact]
    public void ConfirmPassword_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = "ValidPass123!",
            ConfirmPassword = string.Empty
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Password confirmation is required");
    }

    [Fact]
    public void ConfirmPassword_WhenDoesNotMatchPassword_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = "ValidPass123!",
            ConfirmPassword = "DifferentPass123!"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Passwords do not match");
    }

    [Fact]
    public void ConfirmPassword_WhenMatchesPassword_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = "ValidPass123!",
            ConfirmPassword = "ValidPass123!"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    #endregion

    #region Full Request Validation Tests

    [Fact]
    public void ValidRequest_WhenAllFieldsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-reset-token-123",
            NewPassword = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            ClientInfo = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
            IpAddress = "192.168.1.100"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ClientInfo_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = "ValidPass123!",
            ConfirmPassword = "ValidPass123!",
            ClientInfo = new string('a', 501)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientInfo)
            .WithErrorMessage("Client info cannot exceed 500 characters");
    }

    [Fact]
    public void IpAddress_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var request = new PasswordResetConfirmRequest 
        { 
            Token = "valid-token",
            NewPassword = "ValidPass123!",
            ConfirmPassword = "ValidPass123!",
            IpAddress = new string('1', 46)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
            .WithErrorMessage("IP address cannot exceed 45 characters");
    }

    #endregion
}
