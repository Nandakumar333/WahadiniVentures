using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Concurrency;

/// <summary>
/// Concurrency tests for discount redemption system
/// Tests: T100 - Simultaneous redemption attempts with optimistic concurrency control
/// </summary>
[Collection("Sequential")]
public class RedemptionConcurrencyTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;

    public RedemptionConcurrencyTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SimultaneousRedemptions_With50Requests_PreventsDoubleSpending()
    {
        // Arrange: Create 50 users and 1 discount with MaxRedemptions=25
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "CONCURRENT_TEST",
            DiscountPercentage = 20,
            RequiredPoints = 100,
            MaxRedemptions = 25, // Only 25 can redeem
            CurrentRedemptions = 0,
            IsActive = true,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);

        var users = new List<User>();
        for (int i = 0; i < 50; i++)
        {
            var user = User.Create($"concurrent{i}@test.com", "hash", $"User{i}", "Test");
            user.AwardPoints(200); // Each has sufficient points
            user.ConfirmEmail();
            users.Add(user);
            dbContext.Users.Add(user);
        }

        await dbContext.SaveChangesAsync();

        // Act: Make 50 simultaneous redemption attempts
        var tasks = new List<Task<(bool Success, Guid UserId)>>();

        foreach (var user in users)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var client = _factory.CreateClient();
                    var token = GenerateJwtToken(user.Id, user.Email, new[] { "User" }, new[] { "discounts:redeem" });
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.PostAsync($"/api/discounts/{discount.Id}/redeem", null);
                    return (response.IsSuccessStatusCode, user.Id);
                }
                catch
                {
                    return (false, user.Id);
                }
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        var successfulRedemptions = results.Count(r => r.Success);
        successfulRedemptions.Should().BeLessOrEqualTo(25, "Only 25 redemptions allowed due to MaxRedemptions");

        // Verify database integrity
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var updatedDiscount = await verifyDbContext.DiscountCodes
            .Include(d => d.Redemptions)
            .FirstOrDefaultAsync(d => d.Id == discount.Id);

        updatedDiscount.Should().NotBeNull();
        updatedDiscount!.CurrentRedemptions.Should().Be(successfulRedemptions, "CurrentRedemptions must match actual redemptions");
        updatedDiscount.Redemptions.Count.Should().Be(successfulRedemptions, "Database redemptions must match counter");

        // Verify no duplicate redemptions
        var redemptionsByUser = updatedDiscount.Redemptions.GroupBy(r => r.UserId);
        redemptionsByUser.Should().OnlyContain(g => g.Count() == 1, "Each user should only redeem once");

        // Verify point deductions match redemptions
        var redeemedUserIds = updatedDiscount.Redemptions.Select(r => r.UserId).ToList();
        var redeemedUsers = await verifyDbContext.Users
            .Where(u => redeemedUserIds.Contains(u.Id))
            .ToListAsync();

        redeemedUsers.Should().OnlyContain(u => u.CurrentPoints == 100, "Each successful user should have 100 points (200 - 100)");
    }

    [Fact]
    public async Task ConcurrentRedemptions_BySameUser_PreventsDoubleRedemption()
    {
        // Arrange: 1 user, 1 discount, 10 simultaneous requests
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = User.Create("sameuser@test.com", "hash", "Same", "User");
        user.AwardPoints(1000); // More than enough
        user.ConfirmEmail();
        dbContext.Users.Add(user);

        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "ONCE_PER_USER",
            DiscountPercentage = 20,
            RequiredPoints = 100,
            MaxRedemptions = 0, // Unlimited
            CurrentRedemptions = 0,
            IsActive = true,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);
        await dbContext.SaveChangesAsync();

        // Act: Same user makes 10 simultaneous redemption attempts
        var tasks = new List<Task<bool>>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var client = _factory.CreateClient();
                    var token = GenerateJwtToken(user.Id, user.Email, new[] { "User" }, new[] { "discounts:redeem" });
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.PostAsync($"/api/discounts/{discount.Id}/redeem", null);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert: Only 1 should succeed
        var successCount = results.Count(r => r);
        successCount.Should().Be(1, "User should only be able to redeem once despite concurrent attempts");

        // Verify database
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var redemptions = await verifyDbContext.UserDiscountRedemptions
            .Where(r => r.UserId == user.Id && r.DiscountCodeId == discount.Id)
            .ToListAsync();

        redemptions.Count.Should().Be(1, "Only one redemption record should exist");

        var updatedUser = await verifyDbContext.Users.FindAsync(user.Id);
        updatedUser!.CurrentPoints.Should().Be(900, "Points should only be deducted once (1000 - 100)");
    }

    [Fact]
    public async Task OptimisticConcurrency_WithRowVersionConflict_ReturnsConflictStatus()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = User.Create("rowversion@test.com", "hash", "Row", "Version");
        user.AwardPoints(500);
        user.ConfirmEmail();
        dbContext.Users.Add(user);

        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "CONCURRENT_ROW",
            DiscountPercentage = 20,
            RequiredPoints = 100,
            MaxRedemptions = 2,
            CurrentRedemptions = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);
        await dbContext.SaveChangesAsync();

        // Act: 3 rapid consecutive requests (testing optimistic concurrency)
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();
        var client3 = _factory.CreateClient();

        var token = GenerateJwtToken(user.Id, user.Email, new[] { "User" }, new[] { "discounts:redeem" });
        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client3.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Launch requests with minimal delay
        var task1 = client1.PostAsync($"/api/discounts/{discount.Id}/redeem", null);
        await Task.Delay(5); // Small delay to simulate near-simultaneous requests
        var task2 = client2.PostAsync($"/api/discounts/{discount.Id}/redeem", null);
        await Task.Delay(5);
        var task3 = client3.PostAsync($"/api/discounts/{discount.Id}/redeem", null);

        var responses = await Task.WhenAll(task1, task2, task3);

        // Assert: At least one should fail (likely with 400 BadRequest for "already redeemed")
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        successCount.Should().BeLessOrEqualTo(1, "Only one redemption should succeed");

        var failCount = responses.Count(r => !r.IsSuccessStatusCode);
        failCount.Should().BeGreaterOrEqualTo(2, "At least 2 requests should fail");
    }

    private string GenerateJwtToken(Guid userId, string email, string[] roles, string[]? permissions = null)
    {
        var secretKey = "ThisIsAVerySecureTestingSecretKeyThatIsAtLeast256BitsLong!123456789012345678901234567890";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", userId.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        if (permissions != null)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }
        }

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}

