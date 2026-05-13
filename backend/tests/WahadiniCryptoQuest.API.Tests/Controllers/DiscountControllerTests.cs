using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using Xunit;

namespace WahadiniCryptoQuest.API.Tests.Controllers;

/// <summary>
/// Integration tests for DiscountController redemption endpoint
/// Tests: T099 - POST /api/discounts/{id}/redeem endpoint
/// </summary>
[Collection("Sequential")]
public class DiscountControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DiscountControllerTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task RedeemDiscount_WithValidRequest_ReturnsSuccessWithCode()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create test user with points
        var user = User.Create("test@example.com", "hash", "Test", "User");
        user.AwardPoints(1000);
        user.ConfirmEmail();
        dbContext.Users.Add(user);

        // Create test discount
        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "TESTDISCOUNT20",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 10,
            CurrentRedemptions = 0,
            IsActive = true,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);
        await dbContext.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Email, new[] { "User" }, new[] { "discounts:redeem" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/discounts/{discount.Id}/redeem", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RedemptionResponseDto>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Code.Should().StartWith("TESTDISCOUNT20");
        result.DiscountPercentage.Should().Be(20);
        result.PointsDeducted.Should().Be(500);
        result.RemainingPoints.Should().Be(500);

        // Verify database state
        var updatedUser = await dbContext.Users.FindAsync(user.Id);
        updatedUser!.CurrentPoints.Should().Be(500);

        var updatedDiscount = await dbContext.DiscountCodes.FindAsync(discount.Id);
        updatedDiscount!.CurrentRedemptions.Should().Be(1);

        var redemption = await dbContext.UserDiscountRedemptions
            .FirstOrDefaultAsync(r => r.UserId == user.Id && r.DiscountCodeId == discount.Id);
        redemption.Should().NotBeNull();
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

    [Fact]
    public async Task RedeemDiscount_WithInsufficientPoints_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = User.Create("test2@example.com", "hash", "Test", "User");
        user.AwardPoints(300); // Insufficient
        user.ConfirmEmail();
        dbContext.Users.Add(user);

        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "EXPENSIVE",
            DiscountPercentage = 50,
            RequiredPoints = 1000, // User only has 300
            MaxRedemptions = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);
        await dbContext.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Email, new[] { "User" }, new[] { "discounts:redeem" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/discounts/{discount.Id}/redeem", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Insufficient");
    }

    [Fact]
    public async Task RedeemDiscount_WhenUnauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var discountId = Guid.NewGuid();

        // Act (no auth token)
        var response = await _client.PostAsync($"/api/discounts/{discountId}/redeem", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RedeemDiscount_WithExpiredDiscount_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = User.Create("test3@example.com", "hash", "Test", "User");
        user.AwardPoints(1000);
        user.ConfirmEmail();
        dbContext.Users.Add(user);

        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "EXPIRED",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 10,
            IsActive = true,
            ExpiryDate = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);
        await dbContext.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Email, new[] { "User" }, new[] { "discounts:redeem" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/discounts/{discount.Id}/redeem", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RedeemDiscount_WhenDiscountNotFound_ReturnsNotFound()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = User.Create("test4@example.com", "hash", "Test", "User");
        user.AwardPoints(1000);
        user.ConfirmEmail();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var token = GenerateJwtToken(user.Id, user.Email, new[] { "User" }, new[] { "discounts:redeem" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/discounts/{nonExistentId}/redeem", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

