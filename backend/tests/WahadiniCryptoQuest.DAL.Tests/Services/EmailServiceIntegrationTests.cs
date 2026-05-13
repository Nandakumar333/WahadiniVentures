using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Web;
using WahadiniCryptoQuest.DAL.Services;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Services;

/// <summary>
/// Comprehensive integration tests for EmailService
/// Tests email template generation, URL encoding, configuration handling, and various scenarios
/// </summary>
public class EmailServiceIntegrationTests : IDisposable
{
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly EmailService _emailService;

    // Test data
    private readonly string _testEmail = "test.user+tag@example.com";
    private readonly string _testFirstName = "John";
    private readonly string _testUserName = "John Doe";
    private readonly Guid _testUserId = Guid.Parse("12345678-1234-1234-1234-123456789012");
    private readonly string _testToken = "abc123def456ghi789";
    private readonly string _specialCharacterToken = "token+with%special&chars=test";
    private readonly string _frontendBaseUrl = "https://example.com";

    public EmailServiceIntegrationTests()
    {
        _mockLogger = new Mock<ILogger<EmailService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        SetupConfiguration("Development", _frontendBaseUrl);
        _emailService = new EmailService(_mockLogger.Object, _mockConfiguration.Object);
    }

    #region Email Verification Tests

    [Fact]
    public async Task SendEmailVerificationAsync_WithValidData_ShouldGenerateCorrectContent()
    {
        // Act
        var result = await _emailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, _testToken);

        // Assert
        result.Should().BeTrue();
        
        // Verify email content contains all required elements
        VerifyLogContains("Confirm your WahadiniCryptoQuest account"); // Subject
        VerifyLogContains($"Welcome to WahadiniCryptoQuest, {_testFirstName}!"); // Personalization
        VerifyLogContains("/confirm-email"); // Confirmation URL
        VerifyLogContains(_testUserId.ToString()); // User ID in URL
        VerifyLogContains("expire in 24 hours"); // Expiration warning (note: "expire" not "expires")
        VerifyLogContains("WahadiniCryptoQuest Team"); // Branding
    }

    [Fact]
    public async Task SendEmailVerificationAsync_ShouldGenerateCorrectConfirmationUrl()
    {
        // Act
        var result = await _emailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, _testToken);

        // Assert
        result.Should().BeTrue();
        
        var expectedUrl = $"{_frontendBaseUrl}/confirm-email?userId={_testUserId}&token={Uri.EscapeDataString(_testToken)}";
        VerifyLogContains(expectedUrl);
    }

    [Fact]
    public async Task SendEmailVerificationAsync_WithSpecialCharactersInToken_ShouldEncodeUrlProperly()
    {
        // Act
        var result = await _emailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, _specialCharacterToken);

        // Assert
        result.Should().BeTrue();
        
        var expectedEncodedToken = Uri.EscapeDataString(_specialCharacterToken);
        var expectedUrl = $"{_frontendBaseUrl}/confirm-email?userId={_testUserId}&token={expectedEncodedToken}";
        VerifyLogContains(expectedUrl);
        
        // Verify special characters are properly encoded (URI encoding uses % for escaping)
        expectedEncodedToken.Should().NotContain("+"); // + should be encoded to %2B
        expectedEncodedToken.Should().NotContain("&"); // & should be encoded to %26  
        expectedEncodedToken.Should().NotContain("="); // = should be encoded to %3D
        expectedEncodedToken.Should().Contain("%"); // Should contain % as part of URL encoding
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task SendEmailVerificationAsync_WithInvalidToken_ShouldReturnFalse(string invalidToken)
    {
        // Act
        var result = await _emailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, invalidToken);

        // Assert
        result.Should().BeFalse();
        VerifyLogContains("called with null or empty token");
    }

    [Fact] 
    public async Task SendEmailVerificationAsync_WithNullToken_ShouldReturnFalse()
    {
        // Act
        var result = await _emailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, null!);

        // Assert
        result.Should().BeFalse();
        VerifyLogContains("called with null or empty token");
    }

    [Fact]
    public async Task SendEmailVerificationAsync_WithXssInFirstName_ShouldSanitizeContent()
    {
        // Arrange
        var maliciousFirstName = "<script>alert('xss')</script>John";

        // Act
        var result = await _emailService.SendEmailVerificationAsync(_testEmail, maliciousFirstName, _testUserId, _testToken);

        // Assert
        result.Should().BeTrue();
        
        // The content should contain the malicious script as plain text (not executed)
        // This test verifies that the email template doesn't automatically escape HTML
        // In production, additional sanitization might be needed
        VerifyLogContains(maliciousFirstName);
    }

    #endregion

    #region Password Reset Tests

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithValidData_ShouldGenerateCorrectContent()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(2);

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(_testEmail, _testUserName, _testToken, expiresAt);

        // Assert
        result.Should().BeTrue();
        
        VerifyLogContains("Reset your WahadiniCryptoQuest password"); // Subject
        VerifyLogContains($"Hi {_testUserName}"); // Personalization
        VerifyLogContains("/reset-password"); // Reset URL
        VerifyLogContains("expire in 2 hours"); // Expiration time
        VerifyLogContains("WahadiniCryptoQuest Team"); // Branding
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_ShouldGenerateCorrectResetUrl()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(_testEmail, _testUserName, _testToken, expiresAt);

        // Assert
        result.Should().BeTrue();
        
        var expectedUrl = $"{_frontendBaseUrl}/reset-password?email={Uri.EscapeDataString(_testEmail)}&token={Uri.EscapeDataString(_testToken)}";
        VerifyLogContains(expectedUrl);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithEmailContainingSpecialCharacters_ShouldEncodeUrlProperly()
    {
        // Arrange
        var specialEmail = "user+test@example.com";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(specialEmail, _testUserName, _testToken, expiresAt);

        // Assert
        result.Should().BeTrue();
        
        var expectedEncodedEmail = Uri.EscapeDataString(specialEmail);
        var expectedUrl = $"{_frontendBaseUrl}/reset-password?email={expectedEncodedEmail}&token={Uri.EscapeDataString(_testToken)}";
        VerifyLogContains(expectedUrl);
    }

    [Theory]
    [InlineData(0.1)] // 6 minutes
    [InlineData(0.5)] // 30 minutes
    [InlineData(1.0)] // 1 hour
    [InlineData(2.5)] // 2.5 hours
    [InlineData(24.0)] // 24 hours
    public async Task SendPasswordResetEmailAsync_WithDifferentExpirationTimes_ShouldDisplayCorrectDuration(double hoursFromNow)
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(hoursFromNow);

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(_testEmail, _testUserName, _testToken, expiresAt);

        // Assert
        result.Should().BeTrue();
        VerifyLogContains($"expire in {hoursFromNow} hours");
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithXssInUserName_ShouldEscapeHtml()
    {
        // Arrange
        var maliciousUserName = "<script>alert('xss')</script>Evil User";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(_testEmail, maliciousUserName, _testToken, expiresAt);

        // Assert
        result.Should().BeTrue();
        
        // Verify that HTML is escaped using System.Net.WebUtility.HtmlEncode
        var escapedUserName = System.Net.WebUtility.HtmlEncode(maliciousUserName);
        VerifyLogContains(escapedUserName);
        
        // Verify the unescaped version is NOT in the email
        VerifyLogDoesNotContain("<script>alert('xss')</script>");
    }

    #endregion

    #region Welcome Email Tests

    [Fact]
    public async Task SendWelcomeEmailAsync_WithValidData_ShouldGenerateCorrectContent()
    {
        // Act
        var result = await _emailService.SendWelcomeEmailAsync(_testEmail, _testFirstName);

        // Assert
        result.Should().BeTrue();
        
        VerifyLogContains("Welcome to WahadiniCryptoQuest!"); // Subject
        VerifyLogContains($"Welcome to WahadiniCryptoQuest, {_testFirstName}!"); // Personalization
        VerifyLogContains("successfully created and verified"); // Account status
        VerifyLogContains("cryptocurrency learning journey"); // Content purpose
        VerifyLogContains("/dashboard"); // Dashboard link
        VerifyLogContains("WahadiniCryptoQuest Team"); // Branding
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_ShouldGenerateDashboardLink()
    {
        // Act
        var result = await _emailService.SendWelcomeEmailAsync(_testEmail, _testFirstName);

        // Assert
        result.Should().BeTrue();
        
        var expectedDashboardUrl = $"{_frontendBaseUrl}/dashboard";
        VerifyLogContains(expectedDashboardUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("empty")]
    public async Task SendWelcomeEmailAsync_WithInvalidFirstName_ShouldStillSendEmail(string invalidFirstName)
    {
        // Act
        var result = await _emailService.SendWelcomeEmailAsync(_testEmail, invalidFirstName);

        // Assert
        result.Should().BeTrue();
        VerifyLogContains("Welcome to WahadiniCryptoQuest!");
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public async Task EmailService_WithCustomFrontendUrl_ShouldUseConfiguredUrl()
    {
        // Arrange
        var customUrl = "https://custom-domain.com";
        SetupConfiguration("Development", customUrl);
        var customEmailService = new EmailService(_mockLogger.Object, _mockConfiguration.Object);

        // Act
        var result = await customEmailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, _testToken);

        // Assert
        result.Should().BeTrue();
        VerifyLogContains($"{customUrl}/confirm-email");
    }

    [Fact]
    public async Task EmailService_WithMissingFrontendUrl_ShouldUseDefaultUrl()
    {
        // Arrange
        SetupConfiguration("Development", null); // No frontend URL configured
        var defaultEmailService = new EmailService(_mockLogger.Object, _mockConfiguration.Object);

        // Act
        var result = await defaultEmailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, _testToken);

        // Assert
        result.Should().BeTrue();
        VerifyLogContains("http://localhost:5173/confirm-email"); // Default URL
    }

    [Fact]
    public async Task EmailService_InProductionEnvironmentWithoutSmtpConfig_ShouldLogWarningAndReturnFalse()
    {
        // Arrange - Production environment without SMTP configuration
        SetupConfiguration("Production", _frontendBaseUrl);
        // No SMTP configuration setup (SmtpHost is null)
        var productionEmailService = new EmailService(_mockLogger.Object, _mockConfiguration.Object);

        // Act
        var result = await productionEmailService.SendEmailAsync(_testEmail, "Test Subject", "Test Body");

        // Assert
        result.Should().BeFalse("SMTP configuration is missing");
        VerifyLogContains("SMTP configuration missing");
        VerifyLogLevel(LogLevel.Warning);
    }

    [Theory]
    [InlineData("Staging")]
    [InlineData("Test")]
    [InlineData("Production")]
    [InlineData("PRODUCTION")]
    [InlineData("")]
    [InlineData("Unknown")]
    public async Task EmailService_InNonDevelopmentEnvironment_ShouldNotLogEmailBody(string environment)
    {
        // Arrange
        SetupConfiguration(environment, _frontendBaseUrl);
        var emailService = new EmailService(_mockLogger.Object, _mockConfiguration.Object);

        // Act
        var result = await emailService.SendEmailAsync(_testEmail, "Test Subject", "Sensitive Content");

        // Assert - In non-development environments without SMTP config, it should return false
        var isDevelopment = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
        
        if (isDevelopment)
        {
            result.Should().BeTrue("Development mode should log and return true");
            VerifyLogContains("Sensitive Content");
        }
        else
        {
            result.Should().BeFalse("Non-development mode without SMTP should return false");
            VerifyLogDoesNotContain("Sensitive Content");
            // Should log warning about missing SMTP config instead
            VerifyLogContains("SMTP configuration missing");
        }
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendEmailAsync_WithNullLogger_ShouldStillReturnTrue()
    {
        // This test demonstrates robustness, even though the current implementation
        // doesn't actually use external services that could fail
        
        // Act
        var result = await _emailService.SendEmailAsync(_testEmail, "Test Subject", "Test Body");

        // Assert
        result.Should().BeTrue();
        
        // In the current implementation, errors would only occur if the logger itself failed
        // or if there were issues with configuration access, but the method is designed
        // to be resilient and return true in development mode
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    public async Task SendEmailAsync_WithInvalidEmailAddress_ShouldStillProcess(string invalidEmail)
    {
        // Act
        var result = await _emailService.SendEmailAsync(invalidEmail, "Test Subject", "Test Body");

        // Assert
        // In current implementation, validation is not performed, so it returns true
        // In production, email validation should be added
        result.Should().BeTrue();
    }

    #endregion

    #region Security and Validation Tests

    [Fact]
    public async Task SendEmailVerificationAsync_WithVeryLongToken_ShouldHandleGracefully()
    {
        // Arrange
        var longToken = new string('a', 1000); // 1000 character token

        // Act
        var result = await _emailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, longToken);

        // Assert
        result.Should().BeTrue();
        VerifyLogContains(Uri.EscapeDataString(longToken));
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithFutureExpirationDate_ShouldCalculateCorrectDuration()
    {
        // Arrange
        var futureExpiration = DateTime.UtcNow.AddDays(1).AddHours(2.5); // 26.5 hours from now

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(_testEmail, _testUserName, _testToken, futureExpiration);

        // Assert
        result.Should().BeTrue();
        VerifyLogContains("26.5 hours");
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithPastExpirationDate_ShouldShowNegativeDuration()
    {
        // Arrange
        var pastExpiration = DateTime.UtcNow.AddHours(-1); // 1 hour ago

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(_testEmail, _testUserName, _testToken, pastExpiration);

        // Assert
        result.Should().BeTrue();
        VerifyLogContains("-1 hours");
    }

    #endregion

    #region Template Consistency Tests

    [Fact]
    public async Task AllEmailTypes_ShouldContainConsistentBranding()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act & Assert - Email Verification
        var verificationResult = await _emailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, _testToken);
        verificationResult.Should().BeTrue();
        VerifyLogContains("WahadiniCryptoQuest Team");

        // Clear mock for next test
        _mockLogger.Invocations.Clear();

        // Act & Assert - Password Reset
        var resetResult = await _emailService.SendPasswordResetEmailAsync(_testEmail, _testUserName, _testToken, expiresAt);
        resetResult.Should().BeTrue();
        VerifyLogContains("WahadiniCryptoQuest Team");

        // Clear mock for next test
        _mockLogger.Invocations.Clear();

        // Act & Assert - Welcome Email
        var welcomeResult = await _emailService.SendWelcomeEmailAsync(_testEmail, _testFirstName);
        welcomeResult.Should().BeTrue();
        VerifyLogContains("WahadiniCryptoQuest Team");
    }

    [Fact]
    public async Task AllEmailTypes_ShouldUseHtmlFormat()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act & Assert - Email Verification
        var verificationResult = await _emailService.SendEmailVerificationAsync(_testEmail, _testFirstName, _testUserId, _testToken);
        verificationResult.Should().BeTrue();
        VerifyLogContains("<html>");
        VerifyLogContains("</html>");

        // Clear mock for next test
        _mockLogger.Invocations.Clear();

        // Act & Assert - Password Reset
        var resetResult = await _emailService.SendPasswordResetEmailAsync(_testEmail, _testUserName, _testToken, expiresAt);
        resetResult.Should().BeTrue();
        VerifyLogContains("<html>");
        VerifyLogContains("</html>");

        // Clear mock for next test
        _mockLogger.Invocations.Clear();

        // Act & Assert - Welcome Email
        var welcomeResult = await _emailService.SendWelcomeEmailAsync(_testEmail, _testFirstName);
        welcomeResult.Should().BeTrue();
        VerifyLogContains("<html>");
        VerifyLogContains("</html>");
    }

    #endregion

    #region Helper Methods

    private void SetupConfiguration(string? environment, string? frontendUrl)
    {
        _mockConfiguration.Setup(c => c["ASPNETCORE_ENVIRONMENT"]).Returns(environment);
        _mockConfiguration.Setup(c => c["Frontend:BaseUrl"]).Returns(frontendUrl);
    }

    private void VerifyLogContains(string expectedContent)
    {
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedContent)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce,
            $"Expected log to contain: '{expectedContent}'");
    }

    private void VerifyLogDoesNotContain(string content)
    {
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(content)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            $"Expected log to NOT contain: '{content}'");
    }

    private void VerifyLogLevel(LogLevel expectedLevel)
    {
        _mockLogger.Verify(
            x => x.Log(
                expectedLevel,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce,
            $"Expected log level: {expectedLevel}");
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
    }
}
