using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Data;

/// <summary>
/// Database initializer for seeding initial data
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with initial categories, admin user, and discount codes
    /// </summary>
    public static async System.Threading.Tasks.Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Airdrops",
                    Description = "Learn about crypto airdrops and how to participate",
                    DisplayOrder = 1,
                    IconUrl = "/icons/airdrop.svg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "GameFi",
                    Description = "Gaming and blockchain integration strategies",
                    DisplayOrder = 2,
                    IconUrl = "/icons/gamefi.svg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Task-to-Earn",
                    Description = "Earn cryptocurrency by completing tasks",
                    DisplayOrder = 3,
                    IconUrl = "/icons/task.svg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "DeFi",
                    Description = "Decentralized Finance fundamentals and strategies",
                    DisplayOrder = 4,
                    IconUrl = "/icons/defi.svg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "NFT Strategies",
                    Description = "NFT investment and trading strategies",
                    DisplayOrder = 5,
                    IconUrl = "/icons/nft.svg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // Seed Admin User
        const string adminEmail = "admin@wahadinicryptoquest.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123!");
            
            if (result.Succeeded)
            {
                // Add to admin role if roles are set up
                // await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed Discount Codes
        if (!await context.DiscountCodes.AnyAsync())
        {
            var discountCodes = new List<DiscountCode>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "SAVE10",
                    DiscountPercentage = 10,
                    RequiredPoints = 500,
                    MaxRedemptions = 0, // Unlimited
                    CurrentRedemptions = 0,
                    ExpiryDate = null, // No expiry
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "SAVE20",
                    DiscountPercentage = 20,
                    RequiredPoints = 1000,
                    MaxRedemptions = 0,
                    CurrentRedemptions = 0,
                    ExpiryDate = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "SAVE30",
                    DiscountPercentage = 30,
                    RequiredPoints = 2000,
                    MaxRedemptions = 0,
                    CurrentRedemptions = 0,
                    ExpiryDate = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "WELCOME50",
                    DiscountPercentage = 50,
                    RequiredPoints = 100,
                    MaxRedemptions = 100, // Limited to first 100 users
                    CurrentRedemptions = 0,
                    ExpiryDate = DateTime.UtcNow.AddMonths(3), // Expires in 3 months
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.DiscountCodes.AddRangeAsync(discountCodes);
            await context.SaveChangesAsync();
        }
    }
}
