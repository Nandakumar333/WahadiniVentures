using FluentValidation.TestHelper;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Service.Validators.Auth;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Validators;

/// <summary>
/// Unit tests for RegisterUserValidator
/// Tests all validation rules for user registration input
/// T020B: Tests for User Story 1 - Registration validation
/// </summary>
public class RegisterUserValidatorTests
{
    private readonly RegisterDtoValidator _validator;

    public RegisterUserValidatorTests()
    {
        _validator = new RegisterDtoValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user")]
    public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("Pass1!")] // 6 characters - too short
    [InlineData("Pass12@")] // 7 characters - too short
    public void Validate_WithPasswordLessThan8Characters_ShouldHaveValidationError(string password)
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = password,
            ConfirmPassword = password,
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordWithoutUppercase_ShouldHaveValidationError()
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "securep@ss123", // no uppercase
            ConfirmPassword = "securep@ss123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordWithoutLowercase_ShouldHaveValidationError()
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SECUREP@SS123", // no lowercase
            ConfirmPassword = "SECUREP@SS123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordWithoutDigit_ShouldHaveValidationError()
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ssword", // no digit
            ConfirmPassword = "SecureP@ssword",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordWithoutSpecialCharacter_ShouldHaveValidationError()
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecurePass123", // no special character
            ConfirmPassword = "SecurePass123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordAndConfirmPasswordMismatch_ShouldHaveValidationError()
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "DifferentP@ss123", // mismatch
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyFirstName_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = firstName,
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyLastName_ShouldHaveValidationError(string lastName)
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = lastName,
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("A")] // 1 character - too short
    public void Validate_WithFirstNameLessThan2Characters_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = firstName,
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_WithFirstNameMoreThan50Characters_ShouldHaveValidationError()
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = new string('A', 51), // 51 characters - too long
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123",
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_WithAcceptTermsFalse_ShouldHaveValidationError()
    {
        // Arrange
        var model = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123",
            AcceptTerms = false // Not accepted
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AcceptTerms);
    }
}
