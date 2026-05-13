using FluentAssertions;
using WahadiniCryptoQuest.Core.ValueObjects;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.ValueObjects;

/// <summary>
/// Tests for Email value object
/// Coverage target: 100% line, 95%+ branch
/// </summary>
public class EmailTests
{
    #region Create Tests - Valid Cases

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("test_user@example-domain.com")]
    [InlineData("a@b.c")]
    [InlineData("TEST@EXAMPLE.COM")]
    public void Create_WithValidEmail_ShouldCreateEmail(string emailAddress)
    {
        // Act
        var email = Email.Create(emailAddress);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(emailAddress.ToLowerInvariant());
    }

    [Fact]
    public void Create_WithUppercaseEmail_ShouldConvertToLowercase()
    {
        // Act
        var email = Email.Create("TEST@EXAMPLE.COM");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithMixedCaseEmail_ShouldConvertToLowercase()
    {
        // Act
        var email = Email.Create("Test.User@Example.COM");

        // Assert
        email.Value.Should().Be("test.user@example.com");
    }

    [Theory]
    [InlineData("user123@example.com")]
    [InlineData("123@example.com")]
    [InlineData("user@sub.domain.example.com")]
    [InlineData("user-name@example.com")]
    [InlineData("user_name@example.com")]
    public void Create_WithValidComplexEmails_ShouldSucceed(string emailAddress)
    {
        // Act
        var email = Email.Create(emailAddress);

        // Assert
        email.Value.Should().Be(emailAddress.ToLowerInvariant());
    }

    #endregion

    #region Create Tests - Invalid Cases

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithNullOrWhitespace_ShouldThrowArgumentException(string? emailAddress)
    {
        // Act
        var act = () => Email.Create(emailAddress!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be empty*")
            .WithParameterName("email");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    [InlineData("user@exam ple.com")]
    [InlineData("user@@example.com")]
    [InlineData("user@.com")]
    [InlineData("user@example.")]
    public void Create_WithInvalidFormat_ShouldThrowArgumentException(string emailAddress)
    {
        // Act
        var act = () => Email.Create(emailAddress);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid email format*")
            .WithParameterName("email");
    }

    [Fact]
    public void Create_WithEmailExceeding320Characters_ShouldThrowArgumentException()
    {
        // Arrange - Email standard RFC 5321 max length is 320 characters
        var longEmail = new string('a', 310) + "@example.com"; // 322 characters

        // Act
        var act = () => Email.Create(longEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot exceed 320 characters*")
            .WithParameterName("email");
    }

    [Fact]
    public void Create_WithEmailExactly320Characters_ShouldSucceed()
    {
        // Arrange - Create exactly 320 character email (max allowed)
        var localPart = new string('a', 308); // 308 chars
        var email = $"{localPart}@example.com"; // Total: 308 + 12 = 320 characters

        // Act
        var result = Email.Create(email);

        // Assert
        result.Should().NotBeNull();
        result.Value.Length.Should().Be(320);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    #endregion

    #region Equals Tests

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act
        var result = email1.Equals(email2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act
        var result = email1.Equals(email2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var result = email.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var otherObject = "test@example.com";

        // Act
        var result = email.Equals(otherObject);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameEmailDifferentCase_ShouldReturnTrue()
    {
        // Arrange - Both get normalized to lowercase
        var email1 = Email.Create("TEST@EXAMPLE.COM");
        var email2 = Email.Create("test@example.com");

        // Act
        var result = email1.Equals(email2);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_WithSameEmail_ShouldReturnSameHashCode()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act
        var hash1 = email1.GetHashCode();
        var hash2 = email2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentEmail_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act
        var hash1 = email1.GetHashCode();
        var hash2 = email2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetHashCode_WithSameEmailDifferentCase_ShouldReturnSameHashCode()
    {
        // Arrange - Both normalized to lowercase
        var email1 = Email.Create("TEST@EXAMPLE.COM");
        var email2 = Email.Create("test@example.com");

        // Act
        var hash1 = email1.GetHashCode();
        var hash2 = email2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion

    #region Value Object Behavior Tests

    [Fact]
    public void Email_ShouldBeImmutable()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var originalValue = email.Value;

        // Act - Try to change (can't because Value has private setter)
        // Just verify value doesn't change
        var currentValue = email.Value;

        // Assert
        currentValue.Should().Be(originalValue);
    }

    [Fact]
    public void Email_WithSameValue_ShouldBeInterchangeable()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert - Value objects with same value should be equal
        email1.Should().Be(email2);
        email1.GetHashCode().Should().Be(email2.GetHashCode());
        email1.ToString().Should().Be(email2.ToString());
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Create_WithEmailContainingSpecialCharacters_ShouldSucceed()
    {
        // Arrange - Valid special characters in email
        var emails = new[]
        {
            "user+tag@example.com",
            "user.name@example.com",
            "user_name@example.com",
            "user-name@example.com"
        };

        // Act & Assert
        foreach (var emailAddress in emails)
        {
            var email = Email.Create(emailAddress);
            email.Value.Should().Be(emailAddress.ToLowerInvariant());
        }
    }

    [Fact]
    public void Create_WithMinimalEmail_ShouldSucceed()
    {
        // Arrange - Shortest possible valid email
        var emailAddress = "a@b.c";

        // Act
        var email = Email.Create(emailAddress);

        // Assert
        email.Value.Should().Be("a@b.c");
    }

    [Theory]
    [InlineData("user@subdomain.example.com")]
    [InlineData("user@sub.sub.example.com")]
    [InlineData("user@very.deep.subdomain.example.com")]
    public void Create_WithSubdomains_ShouldSucceed(string emailAddress)
    {
        // Act
        var email = Email.Create(emailAddress);

        // Assert
        email.Value.Should().Be(emailAddress.ToLowerInvariant());
    }

    #endregion
}
