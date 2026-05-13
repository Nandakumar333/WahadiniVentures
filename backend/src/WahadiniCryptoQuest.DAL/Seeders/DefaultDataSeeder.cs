using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;
using UserRoleEntity = WahadiniCryptoQuest.Core.Entities.UserRole;

namespace WahadiniCryptoQuest.DAL.Seeders;

/// <summary>
/// Seeds default data into the database for development and testing
/// </summary>
public class DefaultDataSeeder
{
    private readonly ApplicationDbContext _context;

    public DefaultDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds all default data
    /// </summary>
    public async Task SeedAsync()
    {
        // Seed RBAC data first (roles and permissions)
        var rbacSeeder = new RBACDataSeeder(_context);
        await rbacSeeder.SeedAsync();

        // Clear tracker to prevent conflicts with Role entities
        _context.ChangeTracker.Clear();

        // Seed SuperAdmin user first (highest privilege)
        await SeedSuperAdminUserAsync();
        _context.ChangeTracker.Clear();

        // Then seed admin user with role assignment
        await SeedAdminUserAsync();
        _context.ChangeTracker.Clear();

        // Seed categories for course organization
        await SeedCategoriesAsync();
        _context.ChangeTracker.Clear();

        // Seed discount codes for reward redemption
        await SeedDiscountCodesAsync();
        _context.ChangeTracker.Clear();

        // Seed sample courses and lessons
        var courseSeeder = new CourseDataSeeder(_context);
        await courseSeeder.SeedAsync();
        _context.ChangeTracker.Clear();

        // Seed test users for development
        await SeedTestUsersAsync();
        _context.ChangeTracker.Clear();

        // Seed reward data (transactions and achievements)
        await SeedRewardDataAsync();
        _context.ChangeTracker.Clear();

        // Seed currency pricing
        await SeedCurrencyPricingAsync();
    }

    /// <summary>
    /// Seeds test users for development and testing
    /// </summary>
    private async Task SeedTestUsersAsync()
    {
        // Check if test user already exists
        var existingTestUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "test@wahadinicryptoquest.com");

        if (existingTestUser != null)
            return;

        // Create regular test user (Free tier)
        var testUser = User.Create(
            email: "test@wahadinicryptoquest.com",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Test@123"),
            firstName: "Test",
            lastName: "User"
        );

        testUser.ConfirmEmail(); // Pre-confirm test user email
        testUser.AssignRole(UserRoleEnum.Free); // Free tier user

        _context.Users.Add(testUser);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Assign Free role to the test user in UserRole table
        var freeRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Free");
        var savedTestUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@wahadinicryptoquest.com");

        if (freeRole != null && savedTestUser != null)
        {
            var userRole = UserRoleEntity.Create(savedTestUser, freeRole, expiresAt: null);
            _context.Set<UserRoleEntity>().Add(userRole);

            _context.Entry(freeRole).State = EntityState.Unchanged;
            _context.Entry(savedTestUser).State = EntityState.Unchanged;

            await _context.SaveChangesAsync();
        }

        _context.ChangeTracker.Clear();

        // Create premium test user
        var premiumUser = User.Create(
            email: "premium@wahadinicryptoquest.com",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Premium@123"),
            firstName: "Premium",
            lastName: "User"
        );

        premiumUser.ConfirmEmail();
        premiumUser.AssignRole(UserRoleEnum.Premium);

        _context.Users.Add(premiumUser);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Assign Premium role to the premium user in UserRole table
        var premiumRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Premium");
        var savedPremiumUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "premium@wahadinicryptoquest.com");

        if (premiumRole != null && savedPremiumUser != null)
        {
            var userRole = UserRoleEntity.Create(savedPremiumUser, premiumRole, expiresAt: null);
            _context.Set<UserRoleEntity>().Add(userRole);

            _context.Entry(premiumRole).State = EntityState.Unchanged;
            _context.Entry(savedPremiumUser).State = EntityState.Unchanged;

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds a default SuperAdmin user for development with SuperAdmin role
    /// </summary>
    private async Task SeedSuperAdminUserAsync()
    {
        // Check if SuperAdmin user already exists
        var existingSuperAdmin = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "admin@wahadini.com");

        if (existingSuperAdmin != null)
        {
            // Update existing SuperAdmin user if role is not set correctly
            if (existingSuperAdmin.Role != UserRoleEnum.SuperAdmin)
            {
                existingSuperAdmin.AssignRole(UserRoleEnum.SuperAdmin);
                _context.Users.Update(existingSuperAdmin);
                await _context.SaveChangesAsync();
            }

            // Ensure UserRole assignment exists
            var superAdminRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            if (superAdminRole != null)
            {
                var existingUserRole = await _context.Set<UserRoleEntity>()
                    .FirstOrDefaultAsync(ur => ur.UserId == existingSuperAdmin.Id && ur.RoleId == superAdminRole.Id);

                if (existingUserRole == null)
                {
                    // Clear tracker to ensure clean state
                    _context.ChangeTracker.Clear();

                    // Reload entities to ensure clean tracking
                    var cleanSuperAdminRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
                    var cleanSuperAdminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@wahadini.com");

                    if (cleanSuperAdminRole != null && cleanSuperAdminUser != null)
                    {
                        var userRole = UserRoleEntity.Create(cleanSuperAdminUser, cleanSuperAdminRole, expiresAt: null);
                        _context.Set<UserRoleEntity>().Add(userRole);

                        // Explicitly ensure referenced entities are not marked as Added
                        _context.Entry(cleanSuperAdminRole).State = EntityState.Unchanged;
                        _context.Entry(cleanSuperAdminUser).State = EntityState.Unchanged;

                        await _context.SaveChangesAsync();
                    }
                }
            }

            return;
        }

        // Create SuperAdmin user
        var superAdminUser = User.Create(
            email: "admin@wahadini.com",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin@12345"), // Default password per quickstart.md
            firstName: "Super",
            lastName: "Admin"
        );

        superAdminUser.ConfirmEmail(); // Pre-confirm SuperAdmin email
        superAdminUser.AssignRole(UserRoleEnum.SuperAdmin); // Set the role to SuperAdmin

        _context.Users.Add(superAdminUser);
        await _context.SaveChangesAsync();

        // Clear tracker to ensure clean state for role assignment
        _context.ChangeTracker.Clear();

        // Assign SuperAdmin role to the SuperAdmin user in UserRole table
        var superAdminRole2 = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        var savedSuperAdminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@wahadini.com");

        if (superAdminRole2 != null && savedSuperAdminUser != null)
        {
            var userRole = UserRoleEntity.Create(savedSuperAdminUser, superAdminRole2, expiresAt: null); // No expiration for SuperAdmin
            _context.Set<UserRoleEntity>().Add(userRole);

            // Explicitly ensure referenced entities are not marked as Added
            _context.Entry(superAdminRole2).State = EntityState.Unchanged;
            _context.Entry(savedSuperAdminUser).State = EntityState.Unchanged;

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds a default admin user for development with Admin role
    /// </summary>
    private async Task SeedAdminUserAsync()
    {
        // Check if admin user already exists
        var existingAdmin = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "admin@wahadinicryptoquest.com");

        if (existingAdmin != null)
        {
            // Update existing admin user if role is not set correctly
            if (existingAdmin.Role != UserRoleEnum.Admin)
            {
                existingAdmin.AssignRole(UserRoleEnum.Admin);
                _context.Users.Update(existingAdmin);
                await _context.SaveChangesAsync();
            }

            // Ensure UserRole assignment exists
            var adminRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var existingUserRole = await _context.Set<UserRoleEntity>()
                    .FirstOrDefaultAsync(ur => ur.UserId == existingAdmin.Id && ur.RoleId == adminRole.Id);

                if (existingUserRole == null)
                {
                    // Clear tracker to ensure clean state
                    _context.ChangeTracker.Clear();

                    // Reload entities to ensure clean tracking
                    var cleanAdminRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Admin");
                    var cleanAdminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@wahadinicryptoquest.com");

                    if (cleanAdminRole != null && cleanAdminUser != null)
                    {
                        var userRole = UserRoleEntity.Create(cleanAdminUser, cleanAdminRole, expiresAt: null);
                        _context.Set<UserRoleEntity>().Add(userRole);

                        // Explicitly ensure referenced entities are not marked as Added
                        _context.Entry(cleanAdminRole).State = EntityState.Unchanged;
                        _context.Entry(cleanAdminUser).State = EntityState.Unchanged;

                        await _context.SaveChangesAsync();
                    }
                }
            }

            return;
        }

        // Create admin user
        var adminUser = User.Create(
            email: "admin@wahadinicryptoquest.com",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Default password, should be changed
            firstName: "Admin",
            lastName: "User"
        );

        adminUser.ConfirmEmail(); // Pre-confirm admin email
        adminUser.AssignRole(UserRoleEnum.Admin); // Set the role to Admin

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        // Clear tracker to ensure clean state for role assignment
        _context.ChangeTracker.Clear();

        // Assign Admin role to the admin user in UserRole table
        var adminRole2 = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Admin");
        var savedAdminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@wahadinicryptoquest.com");

        if (adminRole2 != null && savedAdminUser != null)
        {
            var userRole = UserRoleEntity.Create(savedAdminUser, adminRole2, expiresAt: null); // No expiration for admin
            _context.Set<UserRoleEntity>().Add(userRole);

            // Explicitly ensure referenced entities are not marked as Added
            _context.Entry(adminRole2).State = EntityState.Unchanged;
            _context.Entry(savedAdminUser).State = EntityState.Unchanged;

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds default course categories
    /// </summary>
    private async Task SeedCategoriesAsync()
    {
        // Check if categories already exist
        if (await _context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Airdrops",
                Description = "Learn about crypto airdrops and how to participate in token distributions",
                IconUrl = "/icons/airdrop.svg",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "GameFi",
                Description = "Explore the intersection of gaming and blockchain technology",
                IconUrl = "/icons/gamefi.svg",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Task-to-Earn",
                Description = "Complete tasks and earn cryptocurrency rewards",
                IconUrl = "/icons/task.svg",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "DeFi",
                Description = "Understand Decentralized Finance protocols and opportunities",
                IconUrl = "/icons/defi.svg",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "NFT Strategies",
                Description = "Master NFT investment strategies and digital collectibles",
                IconUrl = "/icons/nft.svg",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds default discount codes for reward point redemption
    /// </summary>
    private async Task SeedDiscountCodesAsync()
    {
        // Check if discount codes already exist
        if (await _context.DiscountCodes.AnyAsync())
            return;

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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "SAVE20",
                DiscountPercentage = 20,
                RequiredPoints = 1000,
                MaxRedemptions = 0, // Unlimited
                CurrentRedemptions = 0,
                ExpiryDate = null, // No expiry
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "SAVE30",
                DiscountPercentage = 30,
                RequiredPoints = 2000,
                MaxRedemptions = 0, // Unlimited
                CurrentRedemptions = 0,
                ExpiryDate = null, // No expiry
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.DiscountCodes.AddRange(discountCodes);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds dummy reward data for the test user
    /// </summary>
    private async Task SeedRewardDataAsync()
    {
        // Get test user
        var testUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@wahadinicryptoquest.com");
        if (testUser == null)
            return;

        // Check if transactions already exist
        if (await _context.Set<RewardTransaction>().AnyAsync(t => t.UserId == testUser.Id))
            return;

        var transactions = new List<RewardTransaction>();
        var currentBalance = 0;

        // 1. Welcome Bonus (Admin Bonus)
        currentBalance += 100;
        transactions.Add(RewardTransaction.Create(
            testUser.Id,
            100,
            TransactionType.AdminBonus,
            "Welcome Bonus",
            currentBalance,
            null,
            null,
            null // System
        ));

        // 2. Lesson Completion
        currentBalance += 50;
        transactions.Add(RewardTransaction.Create(
            testUser.Id,
            50,
            TransactionType.LessonCompletion,
            "Completed lesson: What are Crypto Airdrops?",
            currentBalance,
            Guid.NewGuid().ToString(), // Mock LessonId
            "Lesson"
        ));

        // 3. Daily Streak
        currentBalance += 10;
        transactions.Add(RewardTransaction.Create(
            testUser.Id,
            10,
            TransactionType.DailyStreak,
            "Daily Login Streak (Day 1)",
            currentBalance,
            DateTime.UtcNow.ToString("yyyy-MM-dd"),
            "Streak"
        ));

        _context.Set<RewardTransaction>().AddRange(transactions);

        // Update user total points
        testUser.AwardPoints(currentBalance);
        _context.Users.Update(testUser);

        // Seed Achievements
        if (!await _context.Set<UserAchievement>().AnyAsync(ua => ua.UserId == testUser.Id))
        {
            var achievements = new List<UserAchievement>
            {
                // Unlocked "First Steps"
                new UserAchievement
                {
                    UserId = testUser.Id,
                    AchievementId = "first-steps", // Matches AchievementCatalog
                    UnlockedAt = DateTime.UtcNow.AddDays(-1),
                    IsUnlocked = true,
                    Progress = 100,
                    Notified = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                // In-progress "Course Champion"
                new UserAchievement
                {
                    UserId = testUser.Id,
                    AchievementId = "course-champion",
                    UnlockedAt = DateTime.MinValue, // Not unlocked
                    IsUnlocked = false,
                    Progress = 50, // 50% progress
                    Notified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _context.Set<UserAchievement>().AddRange(achievements);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds default currency pricing data
    /// </summary>
    private async Task SeedCurrencyPricingAsync()
    {
        if (await _context.CurrencyPricings.AnyAsync())
            return;

        // Get admin user for CreatedBy
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@wahadinicryptoquest.com");
        var adminId = adminUser?.Id ?? Guid.Empty;

        var usdPricing = CurrencyPricing.Create(
            currencyCode: "USD",
            stripePriceId: "price_1QTS2fRvxkU8yCJ1HuwjTm4Z",
            monthlyPrice: 29.00m,
            yearlyPrice: 290.00m,
            currencySymbol: "$",
            showDecimal: true,
            decimalPlaces: 2,
            adminUserId: adminId
        );

        _context.CurrencyPricings.Add(usdPricing);
        await _context.SaveChangesAsync();
    }
}
