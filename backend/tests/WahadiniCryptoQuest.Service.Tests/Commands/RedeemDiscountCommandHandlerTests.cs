using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Discount.Commands;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Commands;

/// <summary>
/// Unit tests for RedeemDiscountCommandHandler
/// Tests: T096-T098 - Success, insufficient points, and already redeemed scenarios
/// </summary>
public class RedeemDiscountCommandHandlerTests
{
    private readonly Mock<IDiscountService> _mockDiscountService;
    private readonly Mock<ILogger<RedeemDiscountCommandHandler>> _mockLogger;
    private readonly RedeemDiscountCommandHandler _handler;

    public RedeemDiscountCommandHandlerTests()
    {
        _mockDiscountService = new Mock<IDiscountService>();
        _mockLogger = new Mock<ILogger<RedeemDiscountCommandHandler>>();
        _handler = new RedeemDiscountCommandHandler(_mockDiscountService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenRedemptionSuccessful_ReturnsRedemptionResponseDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var discountCodeId = Guid.NewGuid();
        var command = new RedeemDiscountCommand(userId, discountCodeId);

        var expectedResponse = new RedemptionResponseDto
        {
            Id = Guid.NewGuid(),
            Code = "SAVE20-ABC123",
            DiscountPercentage = 20,
            PointsDeducted = 500,
            RemainingPoints = 1500,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            RedeemedAt = DateTime.UtcNow
        };

        _mockDiscountService
            .Setup(s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Code, result.Code);
        Assert.Equal(expectedResponse.DiscountPercentage, result.DiscountPercentage);
        Assert.Equal(expectedResponse.PointsDeducted, result.PointsDeducted);
        Assert.Equal(expectedResponse.ExpiryDate, result.ExpiryDate);

        _mockDiscountService.Verify(
            s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInsufficientPoints_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var discountCodeId = Guid.NewGuid();
        var command = new RedeemDiscountCommand(userId, discountCodeId);

        _mockDiscountService
            .Setup(s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Insufficient balance: 300 < 500"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Insufficient balance", exception.Message);

        _mockDiscountService.Verify(
            s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyRedeemed_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var discountCodeId = Guid.NewGuid();
        var command = new RedeemDiscountCommand(userId, discountCodeId);

        _mockDiscountService
            .Setup(s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User has already redeemed this discount"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("already redeemed", exception.Message);

        _mockDiscountService.Verify(
            s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDiscountExpired_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var discountCodeId = Guid.NewGuid();
        var command = new RedeemDiscountCommand(userId, discountCodeId);

        _mockDiscountService
            .Setup(s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Discount code has expired"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("expired", exception.Message);

        _mockDiscountService.Verify(
            s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMaxRedemptionsReached_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var discountCodeId = Guid.NewGuid();
        var command = new RedeemDiscountCommand(userId, discountCodeId);

        _mockDiscountService
            .Setup(s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Maximum redemptions reached"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Maximum redemptions reached", exception.Message);

        _mockDiscountService.Verify(
            s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDiscountNotActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var discountCodeId = Guid.NewGuid();
        var command = new RedeemDiscountCommand(userId, discountCodeId);

        _mockDiscountService
            .Setup(s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Discount code is not active"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("not active", exception.Message);

        _mockDiscountService.Verify(
            s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsInformationOnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var discountCodeId = Guid.NewGuid();
        var command = new RedeemDiscountCommand(userId, discountCodeId);

        var response = new RedemptionResponseDto
        {
            Id = Guid.NewGuid(),
            Code = "SAVE20-ABC123",
            DiscountPercentage = 20,
            PointsDeducted = 500,
            RemainingPoints = 1500,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            RedeemedAt = DateTime.UtcNow
        };

        _mockDiscountService
            .Setup(s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing redemption command")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully redeemed discount")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LogsErrorOnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var discountCodeId = Guid.NewGuid();
        var command = new RedeemDiscountCommand(userId, discountCodeId);

        var expectedException = new InvalidOperationException("Test error");

        _mockDiscountService
            .Setup(s => s.RedeemDiscountAsync(userId, discountCodeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to redeem discount")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
