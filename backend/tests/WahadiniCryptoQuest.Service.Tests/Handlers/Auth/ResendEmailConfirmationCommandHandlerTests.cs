using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Auth;
using WahadiniCryptoQuest.Service.Handlers.Auth;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Handlers.Auth;

public class ResendEmailConfirmationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailVerificationTokenRepository> _tokenRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ResendEmailConfirmationCommandHandler>> _loggerMock;
    private readonly ResendEmailConfirmationCommandHandler _handler;

    public ResendEmailConfirmationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenRepositoryMock = new Mock<IEmailVerificationTokenRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ResendEmailConfirmationCommandHandler>>();

        _unitOfWorkMock.Setup(uow => uow.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(uow => uow.EmailVerificationTokens).Returns(_tokenRepositoryMock.Object);

        _handler = new ResendEmailConfirmationCommandHandler(
            _unitOfWorkMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidEmail_ShouldResendConfirmation()
    {
        // Arrange
        var email = "test@example.com";
        var user = User.Create(email, "hashedPassword", "John", "Doe");
        
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenRepositoryMock.Setup(x => x.InvalidateAllUserTokensAsync(user.Id))
            .Returns(Task.CompletedTask);

        _tokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<EmailVerificationToken>()))
            .ReturnsAsync((EmailVerificationToken token) => token);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock.Setup(x => x.SendEmailVerificationAsync(
            email,
            user.FirstName,
            user.Id,
            It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _tokenRepositoryMock.Verify(x => x.InvalidateAllUserTokensAsync(user.Id), Times.Once);
        _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EmailVerificationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailVerificationAsync(
            email,
            user.FirstName,
            user.Id,
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldReturnTrueToPreventEnumeration()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _tokenRepositoryMock.Verify(x => x.InvalidateAllUserTokensAsync(It.IsAny<Guid>()), Times.Never);
        _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EmailVerificationToken>()), Times.Never);
        _emailServiceMock.Verify(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyConfirmedEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "confirmed@example.com";
        var user = User.Create(email, "hashedPassword", "Jane", "Smith");
        user.ConfirmEmail(); // Confirm the email
        
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _tokenRepositoryMock.Verify(x => x.InvalidateAllUserTokensAsync(It.IsAny<Guid>()), Times.Never);
        _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EmailVerificationToken>()), Times.Never);
        _emailServiceMock.Verify(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task Handle_WhenUserRepositoryThrows_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EmailVerificationToken>()), Times.Never);
        _emailServiceMock.Verify(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTokenRepositoryThrows_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var user = User.Create(email, "hashedPassword", "John", "Doe");
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenRepositoryMock.Setup(x => x.InvalidateAllUserTokensAsync(user.Id))
            .ThrowsAsync(new Exception("Token repository error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _emailServiceMock.Verify(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrows_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var user = User.Create(email, "hashedPassword", "John", "Doe");
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenRepositoryMock.Setup(x => x.InvalidateAllUserTokensAsync(user.Id))
            .Returns(Task.CompletedTask);

        _tokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<EmailVerificationToken>()))
            .ReturnsAsync((EmailVerificationToken token) => token);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock.Setup(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()))
            .ThrowsAsync(new Exception("Email service error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Token Management

    [Fact]
    public async Task Handle_ShouldInvalidateAllExistingTokens_BeforeCreatingNew()
    {
        // Arrange
        var email = "test@example.com";
        var user = User.Create(email, "hashedPassword", "John", "Doe");
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        var invalidateCallOrder = 0;
        var addTokenCallOrder = 0;

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenRepositoryMock.Setup(x => x.InvalidateAllUserTokensAsync(user.Id))
            .Callback(() => invalidateCallOrder = 1)
            .Returns(Task.CompletedTask);

        _tokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<EmailVerificationToken>()))
            .Callback(() => addTokenCallOrder = invalidateCallOrder + 1)
            .ReturnsAsync((EmailVerificationToken token) => token);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock.Setup(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        invalidateCallOrder.Should().Be(1);
        addTokenCallOrder.Should().Be(2);
        _tokenRepositoryMock.Verify(x => x.InvalidateAllUserTokensAsync(user.Id), Times.Once);
        _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EmailVerificationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateTokenWith24HoursExpiration()
    {
        // Arrange
        var email = "test@example.com";
        var user = User.Create(email, "hashedPassword", "John", "Doe");
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        EmailVerificationToken? capturedToken = null;

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenRepositoryMock.Setup(x => x.InvalidateAllUserTokensAsync(user.Id))
            .Returns(Task.CompletedTask);

        _tokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<EmailVerificationToken>()))
            .Callback<EmailVerificationToken>(token => capturedToken = token)
            .ReturnsAsync((EmailVerificationToken token) => token);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock.Setup(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        capturedToken.Should().NotBeNull();
        capturedToken!.UserId.Should().Be(user.Id);
        capturedToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Logging

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenProcessingRequest()
    {
        // Arrange
        var email = "test@example.com";
        var user = User.Create(email, "hashedPassword", "John", "Doe");
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenRepositoryMock.Setup(x => x.InvalidateAllUserTokensAsync(user.Id))
            .Returns(Task.CompletedTask);

        _tokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<EmailVerificationToken>()))
            .ReturnsAsync((EmailVerificationToken token) => token);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock.Setup(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing resend email confirmation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenEmailNotFound()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("non-existent email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var email = "test@example.com";
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during resend email confirmation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Email Service Integration

    [Fact]
    public async Task Handle_ShouldSendEmailWithCorrectParameters()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var user = User.Create(email, "hashedPassword", firstName, "Doe");
        var command = new ResendEmailConfirmationCommand(new ResendEmailConfirmationDto { Email = email });

        string? capturedEmail = null;
        string? capturedFirstName = null;
        Guid? capturedUserId = null;
        string? capturedToken = null;

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenRepositoryMock.Setup(x => x.InvalidateAllUserTokensAsync(user.Id))
            .Returns(Task.CompletedTask);

        _tokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<EmailVerificationToken>()))
            .ReturnsAsync((EmailVerificationToken token) => token);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock.Setup(x => x.SendEmailVerificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()))
            .Callback<string, string, Guid, string>((e, fn, uid, t) =>
            {
                capturedEmail = e;
                capturedFirstName = fn;
                capturedUserId = uid;
                capturedToken = t;
            })
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        capturedEmail.Should().Be(email);
        capturedFirstName.Should().Be(firstName);
        capturedUserId.Should().Be(user.Id);
        capturedToken.Should().NotBeNullOrEmpty();
    }

    #endregion
}


