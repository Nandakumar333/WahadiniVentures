using FluentValidation.TestHelper;
using WahadiniCryptoQuest.Service.Commands.Auth;
using WahadiniCryptoQuest.Service.Validators.Auth;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Validators;

/// <summary>
/// Unit tests for LoginUserValidator
/// Tests validation rules for user login command
/// </summary>
public class LoginUserValidatorTests
{
    private readonly LoginUserValidator _validator;

    public LoginUserValidatorTests()
    {
        _validator = new LoginUserValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var command = new LoginUserCommand { Email = "", Password = "ValidPassword123!" };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData(null!)]
    public void Should_Have_Error_When_Email_Is_Null(string email)
    {
        // Arrange
        var command = new LoginUserCommand { Email = email, Password = "ValidPassword123!" };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid_Format()
    {
        // Arrange
        var command = new LoginUserCommand { Email = "invalid-email", Password = "ValidPassword123!" };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Please enter a valid email address");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Exceeds_Maximum_Length()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // 261 characters
        var command = new LoginUserCommand { Email = longEmail, Password = "ValidPassword123!" };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 254 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var command = new LoginUserCommand { Email = "test@example.com", Password = "" };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Theory]
    [InlineData(null!)]
    public void Should_Have_Error_When_Password_Is_Null(string password)
    {
        // Arrange
        var command = new LoginUserCommand { Email = "test@example.com", Password = password };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_RememberMe_Is_True()
    {
        // Arrange
        var command = new LoginUserCommand 
        { 
            Email = "test@example.com", 
            Password = "ValidPassword123!",
            RememberMe = true
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.RememberMe);
    }

    [Fact]
    public void Should_Not_Have_Error_When_RememberMe_Is_False()
    {
        // Arrange
        var command = new LoginUserCommand 
        { 
            Email = "test@example.com", 
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.RememberMe);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Properties_Are_Valid()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email+tag@domain.co.uk")]
    [InlineData("simple@example.org")]
    public void Should_Accept_Valid_Email_Formats(string validEmail)
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = validEmail,
            Password = "ValidPassword123!",
            RememberMe = false
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
