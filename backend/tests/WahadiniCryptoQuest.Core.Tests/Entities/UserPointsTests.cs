using WahadiniCryptoQuest.Core.Entities;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.Entities;

/// <summary>
/// Unit tests for User entity point management business logic
/// Tests: T095 - User.DeductPoints() with insufficient balance validation
/// </summary>
public class UserPointsTests
{
    [Fact]
    public void DeductPoints_WhenSufficientBalance_DeductsSuccessfully()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedpassword123",
            "John",
            "Doe"
        );
        user.AwardPoints(1000); // Give user 1000 points

        // Act
        user.DeductPoints(500);

        // Assert
        Assert.Equal(500, user.CurrentPoints);
    }

    [Fact]
    public void DeductPoints_WhenInsufficientBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedpassword123",
            "John",
            "Doe"
        );
        user.AwardPoints(300); // Only 300 points

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => user.DeductPoints(500));
        Assert.Contains("Insufficient balance", exception.Message);
        Assert.Contains("300 < 500", exception.Message);
    }

    [Fact]
    public void DeductPoints_WhenExactBalance_ReducesToZero()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedpassword123",
            "John",
            "Doe"
        );
        user.AwardPoints(1000);

        // Act
        user.DeductPoints(1000);

        // Assert
        Assert.Equal(0, user.CurrentPoints);
    }

    [Fact]
    public void DeductPoints_WhenZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedpassword123",
            "John",
            "Doe"
        );
        user.AwardPoints(1000);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => user.DeductPoints(0));
        Assert.Contains("must be positive", exception.Message);
    }

    [Fact]
    public void DeductPoints_WhenNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedpassword123",
            "John",
            "Doe"
        );
        user.AwardPoints(1000);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => user.DeductPoints(-100));
        Assert.Contains("must be positive", exception.Message);
    }

    [Fact]
    public void DeductPoints_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedpassword123",
            "John",
            "Doe"
        );
        user.AwardPoints(1000);
        var originalUpdatedAt = user.UpdatedAt;

        // Small delay to ensure timestamp changes
        System.Threading.Thread.Sleep(10);

        // Act
        user.DeductPoints(500);

        // Assert
        Assert.True(user.UpdatedAt > originalUpdatedAt, "UpdatedAt should be updated after point deduction");
    }

    [Fact]
    public void DeductPoints_DoesNotAffectTotalPointsEarned()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedpassword123",
            "John",
            "Doe"
        );
        user.AwardPoints(1000);
        var totalEarned = user.TotalPointsEarned;

        // Act
        user.DeductPoints(500);

        // Assert
        Assert.Equal(totalEarned, user.TotalPointsEarned);
        Assert.Equal(500, user.CurrentPoints);
    }

    [Fact]
    public void AwardPoints_ThenDeductPoints_WorksCorrectly()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedpassword123",
            "John",
            "Doe"
        );

        // Act
        user.AwardPoints(500);
        user.AwardPoints(300);
        user.DeductPoints(200);
        user.AwardPoints(100);
        user.DeductPoints(400);

        // Assert
        Assert.Equal(300, user.CurrentPoints); // 500 + 300 - 200 + 100 - 400 = 300
        Assert.Equal(900, user.TotalPointsEarned); // 500 + 300 + 100 = 900
    }

    [Fact]
    public void DeductPoints_WithZeroBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "hashedpassword123",
            "John",
            "Doe"
        );
        // User has 0 points by default

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => user.DeductPoints(1));
        Assert.Contains("Insufficient balance", exception.Message);
        Assert.Contains("0 < 1", exception.Message);
    }
}
