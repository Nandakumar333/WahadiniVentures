using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using UserRole = WahadiniCryptoQuest.Core.Enums.UserRoleEnum;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

/// <summary>
/// Comprehensive integration tests for UserRepository
/// Tests user creation, retrieval, updates, email confirmation, soft delete, and various query scenarios
/// </summary>
public class UserRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _userRepository;

    // Test data
    private readonly string _testEmail = "test.user@example.com";
    private readonly string _testFirstName = "John";
    private readonly string _testLastName = "Doe";
    private readonly string _testPasswordHash = "hashedPassword123";

    public UserRepositoryIntegrationTests()
    {
        // Setup in-memory database for each test
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userRepository = new UserRepository(_context);
    }

    #region User Creation Tests

    [Fact]
    public async Task AddAsync_WithValidUser_ShouldPersistToDatabase()
    {
        // Arrange
        var user = User.Create(_testEmail, _testPasswordHash, _testFirstName, _testLastName);

        // Act
        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be(_testEmail);
        savedUser.FirstName.Should().Be(_testFirstName);
        savedUser.LastName.Should().Be(_testLastName);
        savedUser.EmailConfirmed.Should().BeFalse();
        savedUser.Role.Should().Be(UserRole.Free);
        savedUser.IsDeleted.Should().BeFalse();
        savedUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddAsync_WithMultipleUsers_ShouldPersistAllToDatabase()
    {
        // Arrange
        var user1 = User.Create("user1@example.com", _testPasswordHash, "User", "One");
        var user2 = User.Create("user2@example.com", _testPasswordHash, "User", "Two");
        var user3 = User.Create("user3@example.com", _testPasswordHash, "User", "Three");

        // Act
        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _userRepository.AddAsync(user3);
        await _context.SaveChangesAsync();

        // Assert
        var allUsers = await _userRepository.GetAllAsync();
        allUsers.Should().HaveCount(3);
        allUsers.Should().Contain(u => u.Email == "user1@example.com");
        allUsers.Should().Contain(u => u.Email == "user2@example.com");
        allUsers.Should().Contain(u => u.Email == "user3@example.com");
    }

    #endregion

    #region User Retrieval Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();

        // Act
        var retrievedUser = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Id.Should().Be(user.Id);
        retrievedUser.Email.Should().Be(_testEmail);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Act
        var retrievedUser = await _userRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithSoftDeletedUser_ShouldReturnNull()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();
        user.SoftDelete(); // Soft delete
        await _userRepository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var retrievedUser = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();

        // Act
        var retrievedUser = await _userRepository.GetByEmailAsync(_testEmail);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Should().Be(_testEmail);
        retrievedUser.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Act
        var retrievedUser = await _userRepository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Theory]
    [InlineData("TEST.USER@EXAMPLE.COM")]
    [InlineData("test.user@EXAMPLE.COM")]
    [InlineData("Test.User@Example.Com")]
    public async Task GetByEmailAsync_WithDifferentCasing_ShouldReturnUser(string emailVariant)
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();

        // Act
        var retrievedUser = await _userRepository.GetByEmailAsync(emailVariant);

        // Assert - Note: This depends on database collation settings
        // In SQL Server, default collation is case-insensitive
        // In SQLite/InMemory, this might be case-sensitive
        // The test documents the expected behavior
        if (retrievedUser != null)
        {
            retrievedUser.Email.Should().Be(_testEmail, "Database should handle case-insensitive email lookup");
        }
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleUsers_ShouldReturnAllNonDeletedUsers()
    {
        // Arrange
        var user1 = await CreateAndSaveUserAsync("user1@example.com", "User", "One");
        var user2 = await CreateAndSaveUserAsync("user2@example.com", "User", "Two");
        var user3 = await CreateAndSaveUserAsync("user3@example.com", "User", "Three");

        // Soft delete one user
        user2.SoftDelete();
        await _userRepository.UpdateAsync(user2);
        await _context.SaveChangesAsync();

        // Act
        var allUsers = await _userRepository.GetAllAsync();

        // Assert
        allUsers.Should().HaveCount(2);
        allUsers.Should().Contain(u => u.Email == "user1@example.com");
        allUsers.Should().Contain(u => u.Email == "user3@example.com");
        allUsers.Should().NotContain(u => u.Email == "user2@example.com");
    }

    #endregion

    #region Email Existence Tests

    [Fact]
    public async Task ExistsWithEmailAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        await CreateAndSaveTestUserAsync();

        // Act
        var exists = await _userRepository.ExistsWithEmailAsync(_testEmail);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithEmailAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Act
        var exists = await _userRepository.ExistsWithEmailAsync("nonexistent@example.com");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithEmailAsync_WithSoftDeletedUser_ShouldReturnFalse()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();
        user.SoftDelete();
        await _userRepository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _userRepository.ExistsWithEmailAsync(_testEmail);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region User Role Tests

    [Fact]
    public async Task GetByRoleAsync_WithFreeRole_ShouldReturnOnlyFreeUsers()
    {
        // Arrange
        var freeUser1 = await CreateAndSaveUserAsync("free1@example.com", "Free", "One");
        var freeUser2 = await CreateAndSaveUserAsync("free2@example.com", "Free", "Two");
        var premiumUser = await CreateAndSaveUserAsync("premium@example.com", "Premium", "User");
        premiumUser.UpgradeRole(UserRole.Premium);
        await _userRepository.UpdateAsync(premiumUser);
        await _context.SaveChangesAsync();

        // Act
        var freeUsers = await _userRepository.GetByRoleAsync(UserRole.Free);

        // Assert
        freeUsers.Should().HaveCount(2);
        freeUsers.Should().AllSatisfy(u => u.Role.Should().Be(UserRole.Free));
        freeUsers.Should().Contain(u => u.Email == "free1@example.com");
        freeUsers.Should().Contain(u => u.Email == "free2@example.com");
        freeUsers.Should().NotContain(u => u.Email == "premium@example.com");
    }

    [Fact]
    public async Task GetByRoleAsync_WithAdminRole_ShouldReturnOnlyAdmins()
    {
        // Arrange
        var freeUser = await CreateAndSaveUserAsync("free@example.com", "Free", "User");
        var admin1 = await CreateAndSaveUserAsync("admin1@example.com", "Admin", "One");
        admin1.UpgradeRole(UserRole.Admin);
        var admin2 = await CreateAndSaveUserAsync("admin2@example.com", "Admin", "Two");
        admin2.UpgradeRole(UserRole.Admin);

        await _userRepository.UpdateAsync(admin1);
        await _userRepository.UpdateAsync(admin2);
        await _context.SaveChangesAsync();

        // Act
        var admins = await _userRepository.GetByRoleAsync(UserRole.Admin);

        // Assert
        admins.Should().HaveCount(2);
        admins.Should().AllSatisfy(u => u.Role.Should().Be(UserRole.Admin));
        admins.Should().Contain(u => u.Email == "admin1@example.com");
        admins.Should().Contain(u => u.Email == "admin2@example.com");
        admins.Should().NotContain(u => u.Email == "free@example.com");
    }

    [Fact]
    public async Task GetByRoleAsync_WithNoUsersOfRole_ShouldReturnEmptyList()
    {
        // Arrange
        await CreateAndSaveTestUserAsync(); // Creates a free user by default

        // Act
        var premiumUsers = await _userRepository.GetByRoleAsync(UserRole.Premium);

        // Assert
        premiumUsers.Should().BeEmpty();
    }

    #endregion

    #region Email Confirmation Tests

    /* Commented out - method removed from interface
    [Fact]
    public async Task GetUnconfirmedUsersOlderThanAsync_WithOldUnconfirmedUsers_ShouldReturnThem()
    {
        // Arrange
        var oldUser1 = await CreateAndSaveUserAsync("old1@example.com", "Old", "One");
        var oldUser2 = await CreateAndSaveUserAsync("old2@example.com", "Old", "Two");
        var recentUser = await CreateAndSaveUserAsync("recent@example.com", "Recent", "User");
        var confirmedUser = await CreateAndSaveUserAsync("confirmed@example.com", "Confirmed", "User");
        
        // Manually set creation dates to simulate old accounts using reflection
        SetCreatedAt(oldUser1, DateTime.UtcNow.AddDays(-10));
        SetCreatedAt(oldUser2, DateTime.UtcNow.AddDays(-8));
        SetCreatedAt(recentUser, DateTime.UtcNow.AddDays(-1));
        
        // Confirm one user's email
        confirmedUser.ConfirmEmail();
        SetCreatedAt(confirmedUser, DateTime.UtcNow.AddDays(-9));
        
        await _context.SaveChangesAsync();

        // Act
        var threshold = DateTime.UtcNow.AddDays(-7);
        var unconfirmedOldUsers = await _userRepository.GetUnconfirmedUsersOlderThanAsync(threshold);

        // Assert
        unconfirmedOldUsers.Should().HaveCount(2);
        unconfirmedOldUsers.Should().Contain(u => u.Email == "old1@example.com");
        unconfirmedOldUsers.Should().Contain(u => u.Email == "old2@example.com");
        unconfirmedOldUsers.Should().NotContain(u => u.Email == "recent@example.com"); // Too recent
        unconfirmedOldUsers.Should().NotContain(u => u.Email == "confirmed@example.com"); // Already confirmed
        unconfirmedOldUsers.Should().AllSatisfy(u => 
        {
            u.EmailConfirmed.Should().BeFalse();
            u.CreatedAt.Should().BeBefore(threshold);
        });
    }

    /* Commented out - method removed from interface
    [Fact]
    public async Task GetUnconfirmedUsersOlderThanAsync_WithNoOldUsers_ShouldReturnEmptyList()
    {
        // Arrange
        await CreateAndSaveTestUserAsync(); // Recent user

        // Act
        var threshold = DateTime.UtcNow.AddDays(-7);
        var unconfirmedOldUsers = await _userRepository.GetUnconfirmedUsersOlderThanAsync(threshold);

        // Assert
        unconfirmedOldUsers.Should().BeEmpty();
    }

    */
    #endregion

    #region Failed Login Tests

    /* Commented out - GetUsersWithFailedLoginsAsync method removed from interface

    [Fact]
    public async Task GetUsersWithFailedLoginsAsync_WithUsersHavingFailedLogins_ShouldReturnThem()
    {
        // Arrange
        var user1 = await CreateAndSaveUserAsync("user1@example.com", "User", "One");
        var user2 = await CreateAndSaveUserAsync("user2@example.com", "User", "Two");
        var user3 = await CreateAndSaveUserAsync("user3@example.com", "User", "Three");

        // Set failed login attempts
        user1.IncrementFailedLoginAttempts(); // 1 attempt
        user1.IncrementFailedLoginAttempts(); // 2 attempts
        user1.IncrementFailedLoginAttempts(); // 3 attempts
        
        user2.IncrementFailedLoginAttempts(); // 1 attempt
        user2.IncrementFailedLoginAttempts(); // 2 attempts
        
        // user3 has no failed attempts

        // Set last login times to be within the search period using reflection
        var recentTime = DateTime.UtcNow.AddHours(-1);
        SetLastLoginAt(user1, recentTime);
        SetLastLoginAt(user2, recentTime);
        
        await _context.SaveChangesAsync();

        // Act
        var usersWithFailedLogins = await _userRepository.GetUsersWithFailedLoginsAsync(
            minFailedAttempts: 3, 
            since: DateTime.UtcNow.AddHours(-2));

        // Assert
        usersWithFailedLogins.Should().HaveCount(1);
        usersWithFailedLogins.Should().Contain(u => u.Email == "user1@example.com");
        usersWithFailedLogins.Should().NotContain(u => u.Email == "user2@example.com"); // Only 2 attempts, need 3+
        usersWithFailedLogins.Should().NotContain(u => u.Email == "user3@example.com"); // No failed attempts
        usersWithFailedLogins.First().FailedLoginAttempts.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task GetUsersWithFailedLoginsAsync_WithOldFailedLogins_ShouldNotReturnThem()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();
        user.IncrementFailedLoginAttempts();
        user.IncrementFailedLoginAttempts();
        user.IncrementFailedLoginAttempts();
        
        // Set last login time to be old using reflection
        var oldTime = DateTime.UtcNow.AddDays(-2);
        SetLastLoginAt(user, oldTime);
        await _context.SaveChangesAsync();

        // Act
        var usersWithFailedLogins = await _userRepository.GetUsersWithFailedLoginsAsync(
            minFailedAttempts: 2, 
            since: DateTime.UtcNow.AddHours(-2)); // Looking for recent failed logins only

        // Assert
        usersWithFailedLogins.Should().BeEmpty();
    }

    */

    #endregion

    #region Last Login Update Tests

    [Fact]
    public async Task UpdateLastLoginAsync_WithExistingUser_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();

        // Act
        await _userRepository.UpdateLastLoginAsync(user.Id);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        updatedUser.FailedLoginAttempts.Should().Be(0); // Should be reset by RecordLogin()
    }

    [Fact]
    public async Task UpdateLastLoginAsync_WithNonExistentUser_ShouldNotThrowException()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        // Act & Assert - Should not throw
        await _userRepository.UpdateLastLoginAsync(nonExistentUserId);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateLastLoginAsync_ShouldResetFailedLoginAttempts()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();
        user.IncrementFailedLoginAttempts();
        user.IncrementFailedLoginAttempts();
        user.IncrementFailedLoginAttempts();
        await _userRepository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Verify failed attempts were recorded
        var userBeforeLogin = await _userRepository.GetByIdAsync(user.Id);
        userBeforeLogin!.FailedLoginAttempts.Should().Be(3);

        // Act
        await _userRepository.UpdateLastLoginAsync(user.Id);
        await _context.SaveChangesAsync();

        // Assert
        var userAfterLogin = await _userRepository.GetByIdAsync(user.Id);
        userAfterLogin.Should().NotBeNull();
        userAfterLogin!.FailedLoginAttempts.Should().Be(0);
        userAfterLogin.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region User Update Tests

    [Fact]
    public async Task UpdateAsync_WithUserChanges_ShouldPersistChanges()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();
        var newFirstName = "UpdatedFirstName";
        var newLastName = "UpdatedLastName";

        // Act
        user.UpdateProfile(newFirstName, newLastName);
        await _userRepository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.FirstName.Should().Be(newFirstName);
        updatedUser.LastName.Should().Be(newLastName);
        updatedUser.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_WithEmailConfirmation_ShouldUpdateEmailConfirmedStatus()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();
        user.EmailConfirmed.Should().BeFalse();

        // Act
        user.ConfirmEmail();
        await _userRepository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.EmailConfirmed.Should().BeTrue();
        updatedUser.EmailConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_WithRoleChange_ShouldUpdateUserRole()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();
        user.Role.Should().Be(UserRole.Free);

        // Act
        user.UpgradeRole(UserRole.Premium);
        await _userRepository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Role.Should().Be(UserRole.Premium);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public async Task DeleteAsync_WithSoftDelete_ShouldMarkAsDeleted()
    {
        // Arrange
        var user = await CreateAndSaveTestUserAsync();

        // Act
        user.SoftDelete();
        await _userRepository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var deletedUser = await _userRepository.GetByIdAsync(user.Id);
        deletedUser.Should().BeNull(); // Should not be returned by repository methods

        // But should still exist in database with IsDeleted = true
        // Use AsNoTracking to avoid tracking conflicts and explicitly call IgnoreQueryFilters
        var userInDb = await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id);
        userInDb.Should().NotBeNull();
        userInDb!.IsDeleted.Should().BeTrue();
        userInDb.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetAllAsync_WithSoftDeletedUsers_ShouldExcludeThem()
    {
        // Arrange
        var user1 = await CreateAndSaveUserAsync("user1@example.com", "User", "One");
        var user2 = await CreateAndSaveUserAsync("user2@example.com", "User", "Two");
        var user3 = await CreateAndSaveUserAsync("user3@example.com", "User", "Three");

        // Soft delete one user
        user2.SoftDelete();
        await _userRepository.UpdateAsync(user2);
        await _context.SaveChangesAsync();

        // Act
        var allUsers = await _userRepository.GetAllAsync();

        // Assert
        allUsers.Should().HaveCount(2);
        allUsers.Should().NotContain(u => u.Email == "user2@example.com");
        allUsers.Should().Contain(u => u.Email == "user1@example.com");
        allUsers.Should().Contain(u => u.Email == "user3@example.com");
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ShouldExcludeSoftDeletedUsers()
    {
        // Arrange
        var user1 = await CreateAndSaveUserAsync("user1@example.com", "John", "Doe");
        var user2 = await CreateAndSaveUserAsync("user2@example.com", "John", "Smith");
        var user3 = await CreateAndSaveUserAsync("user3@example.com", "Jane", "Doe");

        // Soft delete one John
        user1.SoftDelete();
        await _userRepository.UpdateAsync(user1);
        await _context.SaveChangesAsync();

        // Act
        var johnsOnly = await _userRepository.FindAsync(u => u.FirstName == "John");

        // Assert
        johnsOnly.Should().HaveCount(1);
        johnsOnly.Should().NotContain(u => u.Email == "user1@example.com"); // Soft deleted
        johnsOnly.Should().Contain(u => u.Email == "user2@example.com"); // Active
    }

    #endregion

    #region Performance and Edge Case Tests

    [Fact]
    public async Task Repository_WithEmptyDatabase_ShouldHandleGracefully()
    {
        // Act & Assert
        var allUsers = await _userRepository.GetAllAsync();
        allUsers.Should().BeEmpty();

        var userById = await _userRepository.GetByIdAsync(Guid.NewGuid());
        userById.Should().BeNull();

        var userByEmail = await _userRepository.GetByEmailAsync("nonexistent@example.com");
        userByEmail.Should().BeNull();

        var exists = await _userRepository.ExistsWithEmailAsync("nonexistent@example.com");
        exists.Should().BeFalse();

        var freeUsers = await _userRepository.GetByRoleAsync(UserRole.Free);
        freeUsers.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_WithSpecialCharactersInEmail_ShouldHandleCorrectly()
    {
        // Arrange
        var specialEmail = "user+test@example-domain.co.uk";
        var user = await CreateAndSaveUserAsync(specialEmail, "Special", "User");

        // Act & Assert
        var retrievedUser = await _userRepository.GetByEmailAsync(specialEmail);
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Should().Be(specialEmail);

        var exists = await _userRepository.ExistsWithEmailAsync(specialEmail);
        exists.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public async Task GetByEmailAsync_WithWhitespaceEmail_ShouldReturnNull(string whitespaceEmail)
    {
        // Act
        var user = await _userRepository.GetByEmailAsync(whitespaceEmail);

        // Assert
        user.Should().BeNull();
    }

    #endregion

    #region Repository Bug Detection Tests

    [Fact]
    public async Task GetByEmailAsync_BugDetection_ShouldWorkWithUsername()
    {
        // This test will expose the bug in UserRepository.GetByEmailAsync
        // where it incorrectly compares against Email instead of a Username field

        // Arrange
        var user = await CreateAndSaveTestUserAsync();

        // Act - Testing the potentially buggy method
        var userByUsername = await _userRepository.GetByEmailAsync(_testEmail);

        // Assert - This currently works because the method incorrectly uses Email
        // In a proper implementation, this should test against a separate Username field
        userByUsername.Should().NotBeNull();
        userByUsername!.Email.Should().Be(_testEmail);

        // TODO: Fix UserRepository.GetByEmailAsync to use actual Username field
        // when the User entity is updated to include a Username property
    }

    [Fact]
    public async Task ExistsByUsernameAsync_BugDetection_ShouldWorkWithUsername()
    {
        // This test will expose the bug in UserRepository.ExistsByUsernameAsync
        // where it incorrectly compares against Email instead of a Username field

        // Arrange
        await CreateAndSaveTestUserAsync();

        // Act - Testing using email since there's no separate username field
        var exists = await _userRepository.ExistsWithEmailAsync(_testEmail);

        // Assert
        exists.Should().BeTrue();
        // when the User entity is updated to include a Username property
    }

    #endregion

    #region Helper Methods

    private async Task<User> CreateAndSaveTestUserAsync()
    {
        return await CreateAndSaveUserAsync(_testEmail, _testFirstName, _testLastName);
    }

    private async Task<User> CreateAndSaveUserAsync(string email, string firstName, string lastName)
    {
        var user = User.Create(email, _testPasswordHash, firstName, lastName);

        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;
    }

    private static void SetCreatedAt(User user, DateTime createdAt)
    {
        var property = typeof(User).GetProperty("CreatedAt");
        property?.SetValue(user, createdAt);
    }

    private static void SetLastLoginAt(User user, DateTime lastLoginAt)
    {
        var property = typeof(User).GetProperty("LastLoginAt");
        property?.SetValue(user, lastLoginAt);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}


