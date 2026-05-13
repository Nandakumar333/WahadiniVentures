using FluentValidation.TestHelper;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Service.Validators.Auth;

namespace WahadiniCryptoQuest.Service.Tests.Validators.Auth;

/// <summary>
/// Tests for EmailConfirmationDtoValidator
/// Validates email confirmation token request validation rules
/// </summary>
public class EmailConfirmationDtoValidatorTests
{
    private readonly EmailConfirmationDtoValidator _validator;

    public EmailConfirmationDtoValidatorTests()
    {
        _validator = new EmailConfirmationDtoValidator();
    }

    #region UserId Validation

    [Fact]
    public void Validate_WithValidUserId_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.NewGuid(),
            Token = "valid-token-123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.Empty,
            Token = "valid-token-123"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required");
    }

    #endregion

    #region Token Validation

    [Fact]
    public void Validate_WithValidToken_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.NewGuid(),
            Token = "valid-token-with-enough-characters"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Validate_WithEmptyToken_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.NewGuid(),
            Token = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Verification token is required");
    }

    [Fact]
    public void Validate_WithNullToken_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.NewGuid(),
            Token = null!
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Verification token is required");
    }

    [Fact]
    public void Validate_WithTokenExceeding500Characters_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.NewGuid(),
            Token = new string('a', 501) // 501 characters
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Token cannot exceed 500 characters");
    }

    [Fact]
    public void Validate_WithTokenExactly500Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.NewGuid(),
            Token = new string('a', 500) // Exactly 500 characters
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Validate_WithWhitespaceToken_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.NewGuid(),
            Token = "   "
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    #endregion

    #region Full DTO Validation

    [Fact]
    public void Validate_WithAllValidFields_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.NewGuid(),
            Token = "valid-token-abc123xyz"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAllInvalidFields_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var dto = new EmailConfirmationDto
        {
            UserId = Guid.Empty,
            Token = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    #endregion
}

/// <summary>
/// Tests for ResendEmailConfirmationDtoValidator
/// Validates resend email confirmation request validation rules
/// </summary>
public class ResendEmailConfirmationDtoValidatorTests
{
    private readonly ResendEmailConfirmationDtoValidator _validator;

    public ResendEmailConfirmationDtoValidatorTests()
    {
        _validator = new ResendEmailConfirmationDtoValidator();
    }

    #region Email Validation

    [Fact]
    public void Validate_WithValidEmail_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new ResendEmailConfirmationDto
        {
            Email = "user@example.com"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new ResendEmailConfirmationDto
        {
            Email = ""
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Validate_WithNullEmail_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new ResendEmailConfirmationDto
        {
            Email = null!
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new ResendEmailConfirmationDto
        {
            Email = "not-an-email"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Please enter a valid email address");
    }

    [Theory]
    [InlineData("invalid.email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Validate_WithVariousInvalidEmailFormats_ShouldHaveValidationError(string email)
    {
        // Arrange
        var dto = new ResendEmailConfirmationDto
        {
            Email = email
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmailExceeding320Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = new string('a', 310) + "@example.com"; // 323 characters
        var dto = new ResendEmailConfirmationDto
        {
            Email = longEmail
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email address cannot exceed 320 characters");
    }

    [Fact]
    public void Validate_WithEmailExactly320Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        // Email format: local@domain
        // 320 chars = 308 local + "@" + 11 domain (.com)
        var localPart = new string('a', 308);
        var email = $"{localPart}@example.com"; // Exactly 320 characters
        var dto = new ResendEmailConfirmationDto
        {
            Email = email
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithWhitespaceEmail_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new ResendEmailConfirmationDto
        {
            Email = "   "
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("user.name@example.com")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("user_name@sub.example.com")]
    [InlineData("123@example.com")]
    [InlineData("user@example-domain.com")]
    public void Validate_WithVariousValidEmailFormats_ShouldNotHaveValidationError(string email)
    {
        // Arrange
        var dto = new ResendEmailConfirmationDto
        {
            Email = email
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Full DTO Validation

    [Fact]
    public void Validate_WithAllValidFields_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var dto = new ResendEmailConfirmationDto
        {
            Email = "user@example.com"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMinimalValidEmail_ShouldNotHaveValidationError()
    {
        // Arrange
        var dto = new ResendEmailConfirmationDto
        {
            Email = "a@b.c"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
