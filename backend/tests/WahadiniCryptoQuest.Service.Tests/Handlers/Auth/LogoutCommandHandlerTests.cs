using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Auth;
using WahadiniCryptoQuest.Service.Handlers.Auth;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Handlers.Auth;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<ILogger<LogoutCommandHandler>> _mockLogger;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _mockLogger = new Mock<ILogger<LogoutCommandHandler>>();
        
        _mockUnitOfWork.Setup(uow => uow.RefreshTokens).Returns(_mockRefreshTokenRepository.Object);
        
        _handler = new LogoutCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_RevokesToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        var tokenString = refreshToken.Token;
        
        var command = new LogoutCommand(tokenString, userId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.RevokedAt.Should().NotBeNull();
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        var tokenString = refreshToken.Token;
        refreshToken.Revoke(userId.ToString());
        
        var command = new LogoutCommand(tokenString, userId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentToken_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "non-existent-token";
        var command = new LogoutCommand(tokenString, userId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyRefreshToken_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new LogoutCommand(string.Empty, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token is required");
        _mockRefreshTokenRepository.Verify(repo => repo.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullRefreshToken_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new LogoutCommand(null!, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token is required");
        _mockRefreshTokenRepository.Verify(repo => repo.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithTokenBelongingToDifferentUser_ReturnsFailure()
    {
        // Arrange
        var tokenOwnerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(tokenOwnerId, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        var tokenString = refreshToken.Token;
        
        var command = new LogoutCommand(tokenString, requestingUserId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid refresh token");
        refreshToken.IsRevoked.Should().BeFalse();
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LogsInformation_OnSuccessfulLogout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        var tokenString = refreshToken.Token;
        
        var command = new LogoutCommand(tokenString, userId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("revoked for user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsWarning_OnEmptyToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new LogoutCommand(string.Empty, userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("empty refresh token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsWarning_OnTokenNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "non-existent-token";
        var command = new LogoutCommand(tokenString, userId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Refresh token not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsWarning_OnTokenBelongingToDifferentUser()
    {
        // Arrange
        var tokenOwnerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(tokenOwnerId, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        var tokenString = refreshToken.Token;
        
        var command = new LogoutCommand(tokenString, requestingUserId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("attempted to revoke token belonging to user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_HandlesRepositoryException_Gracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "valid-token";
        var command = new LogoutCommand(tokenString, userId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("error");
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing logout request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_HandlesSaveChangesException_Gracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        var tokenString = refreshToken.Token;
        
        var command = new LogoutCommand(tokenString, userId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        
        _mockUnitOfWork
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database save error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("error");
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing logout request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SetsRevokedAtInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(userId, DateTime.UtcNow.AddDays(7), "TestDevice", "127.0.0.1");
        var tokenString = refreshToken.Token;
        
        var command = new LogoutCommand(tokenString, userId);
        
        _mockRefreshTokenRepository
            .Setup(repo => repo.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
