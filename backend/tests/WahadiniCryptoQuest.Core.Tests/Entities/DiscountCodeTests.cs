using WahadiniCryptoQuest.Core.Entities;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.Entities;

/// <summary>
/// Unit tests for DiscountCode entity business logic
/// Tests: T094 - DiscountCode.CanRedeem() business logic validation
/// </summary>
public class DiscountCodeTests
{
    [Fact]
    public void CanRedeem_WhenAllConditionsMet_ReturnsTrue()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "SAVE20",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 10,
            CurrentRedemptions = 5,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userPoints = 1000;

        // Act
        var result = discountCode.CanRedeem(userPoints);

        // Assert
        Assert.True(result, "Should allow redemption when all conditions met");
    }

    [Fact]
    public void CanRedeem_WhenDiscountIsInactive_ReturnsFalse()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "INACTIVE",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 10,
            CurrentRedemptions = 0,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsActive = false, // Inactive
            CreatedAt = DateTime.UtcNow
        };

        var userPoints = 1000;

        // Act
        var result = discountCode.CanRedeem(userPoints);

        // Assert
        Assert.False(result, "Should not allow redemption when discount is inactive");
    }

    [Fact]
    public void CanRedeem_WhenExpired_ReturnsFalse()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "EXPIRED",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 10,
            CurrentRedemptions = 0,
            ExpiryDate = DateTime.UtcNow.AddDays(-1), // Expired yesterday
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userPoints = 1000;

        // Act
        var result = discountCode.CanRedeem(userPoints);

        // Assert
        Assert.False(result, "Should not allow redemption when discount is expired");
    }

    [Fact]
    public void CanRedeem_WhenInsufficientPoints_ReturnsFalse()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "EXPENSIVE",
            DiscountPercentage = 50,
            RequiredPoints = 1000,
            MaxRedemptions = 10,
            CurrentRedemptions = 0,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userPoints = 500; // Insufficient

        // Act
        var result = discountCode.CanRedeem(userPoints);

        // Assert
        Assert.False(result, "Should not allow redemption with insufficient points");
    }

    [Fact]
    public void CanRedeem_WhenMaxRedemptionsReached_ReturnsFalse()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "LIMITED",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 10,
            CurrentRedemptions = 10, // At limit
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userPoints = 1000;

        // Act
        var result = discountCode.CanRedeem(userPoints);

        // Assert
        Assert.False(result, "Should not allow redemption when max redemptions reached");
    }

    [Fact]
    public void CanRedeem_WhenExpiryDateIsNull_AllowsRedemption()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "NOEXPIRY",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 10,
            CurrentRedemptions = 0,
            ExpiryDate = null, // No expiry
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userPoints = 1000;

        // Act
        var result = discountCode.CanRedeem(userPoints);

        // Assert
        Assert.True(result, "Should allow redemption when no expiry date set");
    }

    [Fact]
    public void CanRedeem_WhenMaxRedemptionsIsZero_AllowsUnlimitedRedemptions()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "UNLIMITED",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 0, // Unlimited
            CurrentRedemptions = 1000, // Any number
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userPoints = 1000;

        // Act
        var result = discountCode.CanRedeem(userPoints);

        // Assert
        Assert.True(result, "Should allow redemption when MaxRedemptions is 0 (unlimited)");
    }

    [Fact]
    public void CanRedeem_WhenUserHasExactlyRequiredPoints_AllowsRedemption()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "EXACT",
            DiscountPercentage = 20,
            RequiredPoints = 1000,
            MaxRedemptions = 10,
            CurrentRedemptions = 0,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userPoints = 1000; // Exact match

        // Act
        var result = discountCode.CanRedeem(userPoints);

        // Assert
        Assert.True(result, "Should allow redemption when user has exactly required points");
    }

    [Fact]
    public void IncrementRedemptions_WhenWithinLimit_IncrementsSuccessfully()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "INCREMENT",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 10,
            CurrentRedemptions = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        discountCode.IncrementRedemptions();

        // Assert
        Assert.Equal(6, discountCode.CurrentRedemptions);
    }

    [Fact]
    public void IncrementRedemptions_WhenAtMaxLimit_ThrowsInvalidOperationException()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "MAXED",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 10,
            CurrentRedemptions = 10, // At limit
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => discountCode.IncrementRedemptions());
        Assert.Equal("Maximum redemptions reached", exception.Message);
    }

    [Fact]
    public void IncrementRedemptions_WhenUnlimited_IncrementsWithoutLimit()
    {
        // Arrange
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "UNLIMITED_INC",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 0, // Unlimited
            CurrentRedemptions = 9999,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        discountCode.IncrementRedemptions();

        // Assert
        Assert.Equal(10000, discountCode.CurrentRedemptions);
    }
}
