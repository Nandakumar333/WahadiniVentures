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
/// Integration tests for AdminDiscountController CRUD operations
/// Tests: T101 - Admin CRUD operations (Create, Update, Delete, Activate, Deactivate)
/// </summary>
[Collection("Sequential")]
public class AdminDiscountControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdminDiscountControllerTests(TestWebApplicationFactory<Program> factory)
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
    public async Task CreateDiscountCode_WithValidData_ReturnsCreatedDiscount()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateDiscountCodeRequest
        {
            Code = "ADMIN_CREATE20",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            MaxRedemptions = 100,
            ExpiryDate = DateTime.UtcNow.AddDays(60).ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        var content = new StringContent(JsonSerializer.Serialize(createRequest, _jsonOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/admin/discounts", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AdminDiscountTypeDto>(responseContent, _jsonOptions);

        result.Should().NotBeNull();
        result!.Code.Should().Be("ADMIN_CREATE20");
        result.DiscountPercentage.Should().Be(20);
        result.RequiredPoints.Should().Be(500);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateDiscountCode_WithValidData_ReturnsUpdatedDiscount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "UPDATE_TEST",
            DiscountPercentage = 10,
            RequiredPoints = 300,
            MaxRedemptions = 50,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);
        await dbContext.SaveChangesAsync();

        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateDiscountCodeRequest
        {
            DiscountPercentage = 25,
            RequiredPoints = 600
        };

        var content = new StringContent(JsonSerializer.Serialize(updateRequest, _jsonOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/admin/discounts/{discount.Id}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = await verifyDbContext.DiscountCodes.FindAsync(discount.Id);

        updated.Should().NotBeNull();
        updated!.DiscountPercentage.Should().Be(25);
        updated.RequiredPoints.Should().Be(600);
        updated.Code.Should().Be("UPDATE_TEST"); // Unchanged
    }

    [Fact]
    public async Task DeleteDiscountCode_SoftDeletes_PreservesRedemptionHistory()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "DELETE_TEST",
            DiscountPercentage = 15,
            RequiredPoints = 400,
            MaxRedemptions = 20,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);

        // Create a redemption to verify preservation
        var user = User.Create("redemption@test.com", "hash", "Test", "User");
        user.ConfirmEmail();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var redemption = new UserDiscountRedemption
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DiscountCodeId = discount.Id,
            RedeemedAt = DateTime.UtcNow,
            UsedInSubscription = false,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.UserDiscountRedemptions.Add(redemption);
        await dbContext.SaveChangesAsync();

        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync($"/api/admin/discounts/{discount.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Verify soft delete
        var deleted = await verifyDbContext.DiscountCodes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == discount.Id);

        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();

        // Verify redemption still exists
        var preservedRedemption = await verifyDbContext.UserDiscountRedemptions.FindAsync(redemption.Id);
        preservedRedemption.Should().NotBeNull();
    }

    [Fact]
    public async Task ActivateDiscountCode_SetsIsActiveTrue()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "ACTIVATE_TEST",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            IsActive = false, // Start inactive
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);
        await dbContext.SaveChangesAsync();

        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/admin/discounts/{discount.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var activated = await verifyDbContext.DiscountCodes.FindAsync(discount.Id);

        activated.Should().NotBeNull();
        activated!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateDiscountCode_SetsIsActiveFalse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var discount = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "DEACTIVATE_TEST",
            DiscountPercentage = 20,
            RequiredPoints = 500,
            IsActive = true, // Start active
            CreatedAt = DateTime.UtcNow
        };
        dbContext.DiscountCodes.Add(discount);
        await dbContext.SaveChangesAsync();

        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync($"/api/admin/discounts/{discount.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deactivated = await verifyDbContext.DiscountCodes.FindAsync(discount.Id);

        deactivated.Should().NotBeNull();
        deactivated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AdminEndpoints_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange: Get a regular user token
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateDiscountCodeRequest
        {
            Code = "FORBIDDEN_TEST",
            DiscountPercentage = 20,
            RequiredPoints = 500
        };

        var content = new StringContent(JsonSerializer.Serialize(createRequest, _jsonOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/admin/discounts", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #region Helper Methods

    private async Task<string> GetAdminTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "admin@discounttest.com");
        if (adminUser == null)
        {
            var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = Role.Create("Admin", "Administrator");
                dbContext.Roles.Add(adminRole);
                await dbContext.SaveChangesAsync();
            }

            adminUser = User.Create("admin@discounttest.com", "hashedpassword", "Admin", "User");
            adminUser.ConfirmEmail();
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();

            var userRole = Core.Entities.UserRole.Create(adminUser, adminRole);
            dbContext.UserRoles.Add(userRole);
            await dbContext.SaveChangesAsync();
        }

        return GenerateJwtToken(adminUser.Id, adminUser.Email, new[] { "Admin" }, new[] { "discounts:manage", "discounts:create", "discounts:edit", "discounts:delete" });
    }

    private async Task<string> GetUserTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var regularUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "regularuser@discounttest.com");
        if (regularUser == null)
        {
            regularUser = User.Create("regularuser@discounttest.com", "hashedpassword", "Regular", "User");
            regularUser.ConfirmEmail();
            dbContext.Users.Add(regularUser);
            await dbContext.SaveChangesAsync();
        }

        return GenerateJwtToken(regularUser.Id, regularUser.Email, new[] { "User" }, new[] { "discounts:view", "discounts:redeem" });
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

    #endregion
}

// Request DTOs for testing
public class CreateDiscountCodeRequest
{
    public string Code { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public int RequiredPoints { get; set; }
    public int? MaxRedemptions { get; set; }
    public string? ExpiryDate { get; set; }
}

public class UpdateDiscountCodeRequest
{
    public int? DiscountPercentage { get; set; }
    public int? RequiredPoints { get; set; }
    public int? MaxRedemptions { get; set; }
    public string? ExpiryDate { get; set; }
}
