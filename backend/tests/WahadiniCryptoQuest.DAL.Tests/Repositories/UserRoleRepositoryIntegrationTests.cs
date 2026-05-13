using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

/// <summary>
/// Integration tests for UserRoleRepository
/// Tests user-role assignment, retrieval, expiration, activation/deactivation, and soft deletion
/// </summary>
public class UserRoleRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRoleRepository _userRoleRepository;
    private readonly User _testUser;
    private readonly Role _testRole;

    public UserRoleRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userRoleRepository = new UserRoleRepository(_context);

        _testUser = User.Create("test@example.com", "hashedPassword", "John", "Doe");
        _testRole = Role.Create("Premium", "Premium role");
        
        _context.Users.Add(_testUser);
        _context.Roles.Add(_testRole);
        _context.SaveChanges();
    }

    #region AssignRoleAsync and GetByIdAsync Tests

    [Fact]
    public async Task AssignRoleAsync_WithValidUserRole_ShouldPersistToDatabase()
    {
        // Arrange
        var userRole = UserRole.Create(_testUser, _testRole, DateTime.UtcNow.AddDays(30));

        // Act
        await _userRoleRepository.AssignRoleAsync(userRole);
        await _context.SaveChangesAsync();

        // Assert
        var savedUserRole = await _context.Set<UserRole>().FindAsync(userRole.Id);
        savedUserRole.Should().NotBeNull();
        savedUserRole!.UserId.Should().Be(_testUser.Id);
        savedUserRole.RoleId.Should().Be(_testRole.Id);
        savedUserRole.IsActive.Should().BeTrue();
        savedUserRole.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUserRoleWithNavigationProperties()
    {
        // Arrange
        var userRole = UserRole.Create(_testUser, _testRole);
        await _userRoleRepository.AssignRoleAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetByIdAsync(userRole.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userRole.Id);
        result.User.Should().NotBeNull();
        result.Role.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _userRoleRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedUserRole_ShouldReturnNull()
    {
        // Arrange
        var userRole = UserRole.Create(_testUser, _testRole);
        userRole.SoftDelete();
        await _userRoleRepository.AssignRoleAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetByIdAsync(userRole.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserRolesAsync Tests

    [Fact]
    public async Task GetUserRolesAsync_WithMultipleRoles_ShouldReturnAllOrderedByAssignedAtDesc()
    {
        // Arrange
        var role1 = Role.Create("Free", "Free role");
        var role2 = Role.Create("Premium", "Premium role");
        var role3 = Role.Create("VIP", "VIP role");
        
        await _context.Roles.AddRangeAsync(role1, role2, role3);
        await _context.SaveChangesAsync();

        var userRole1 = UserRole.Create(_testUser, role1);
        await Task.Delay(10);
        var userRole2 = UserRole.Create(_testUser, role2);
        await Task.Delay(10);
        var userRole3 = UserRole.Create(_testUser, role3);

        await _userRoleRepository.AssignRoleAsync(userRole1);
        await _userRoleRepository.AssignRoleAsync(userRole2);
        await _userRoleRepository.AssignRoleAsync(userRole3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetUserRolesAsync(_testUser.Id);

        // Assert
        var userRoles = result.ToList();
        userRoles.Should().HaveCount(3);
        userRoles[0].AssignedAt.Should().BeAfter(userRoles[1].AssignedAt); // Ordered descending
        userRoles[0].Role.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldExcludeDeletedRoles()
    {
        // Arrange
        var activeUserRole = UserRole.Create(_testUser, _testRole);
        var deletedUserRole = UserRole.Create(_testUser, _testRole);
        deletedUserRole.SoftDelete();

        await _userRoleRepository.AssignRoleAsync(activeUserRole);
        await _userRoleRepository.AssignRoleAsync(deletedUserRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetUserRolesAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeUserRole.Id);
    }

    [Fact]
    public async Task GetUserRolesAsync_WithNoRoles_ShouldReturnEmpty()
    {
        // Act
        var result = await _userRoleRepository.GetUserRolesAsync(_testUser.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetActiveUserRoleAsync Tests

    [Fact]
    public async Task GetActiveUserRoleAsync_WithActiveRole_ShouldReturnMostRecent()
    {
        // Arrange
        var userRole1 = UserRole.Create(_testUser, _testRole);
        await Task.Delay(10);
        var userRole2 = UserRole.Create(_testUser, _testRole);

        await _userRoleRepository.AssignRoleAsync(userRole1);
        await _userRoleRepository.AssignRoleAsync(userRole2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetActiveUserRoleAsync(_testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userRole2.Id); // Most recent
        result.Role.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveUserRoleAsync_ShouldExcludeInactiveRoles()
    {
        // Arrange
        var activeUserRole = UserRole.Create(_testUser, _testRole);
        await Task.Delay(10);
        var inactiveUserRole = UserRole.Create(_testUser, _testRole);
        inactiveUserRole.Deactivate();

        await _userRoleRepository.AssignRoleAsync(activeUserRole);
        await _userRoleRepository.AssignRoleAsync(inactiveUserRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetActiveUserRoleAsync(_testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(activeUserRole.Id); // Active one returned
    }

    [Fact]
    public async Task GetActiveUserRoleAsync_ShouldExcludeExpiredRoles()
    {
        // Arrange
        var activeUserRole = UserRole.Create(_testUser, _testRole, DateTime.UtcNow.AddDays(30));
        var expiredUserRole = CreateExpiredUserRole(_testUser, _testRole, DateTime.UtcNow.AddDays(-1));

        await _userRoleRepository.AssignRoleAsync(activeUserRole);
        await _userRoleRepository.AssignRoleAsync(expiredUserRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetActiveUserRoleAsync(_testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(activeUserRole.Id); // Non-expired one returned
    }

    [Fact]
    public async Task GetActiveUserRoleAsync_WithNoActiveRoles_ShouldReturnNull()
    {
        // Arrange
        var inactiveUserRole = UserRole.Create(_testUser, _testRole);
        inactiveUserRole.Deactivate();
        await _userRoleRepository.AssignRoleAsync(inactiveUserRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetActiveUserRoleAsync(_testUser.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUsersWithRoleAsync Tests

    [Fact]
    public async Task GetUsersWithRoleAsync_WithMultipleUsers_ShouldReturnAllOrderedByEmail()
    {
        // Arrange
        var user1 = User.Create("zebra@example.com", "hash", "Zebra", "User");
        var user2 = User.Create("alpha@example.com", "hash", "Alpha", "User");
        var user3 = User.Create("beta@example.com", "hash", "Beta", "User");
        
        await _context.Users.AddRangeAsync(user1, user2, user3);
        await _context.SaveChangesAsync();

        var userRole1 = UserRole.Create(user1, _testRole);
        var userRole2 = UserRole.Create(user2, _testRole);
        var userRole3 = UserRole.Create(user3, _testRole);

        await _userRoleRepository.AssignRoleAsync(userRole1);
        await _userRoleRepository.AssignRoleAsync(userRole2);
        await _userRoleRepository.AssignRoleAsync(userRole3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetUsersWithRoleAsync(_testRole.Id);

        // Assert
        var userRoles = result.ToList();
        userRoles.Should().HaveCount(3);
        userRoles[0].User.Email.Should().Be("alpha@example.com"); // Alphabetically first
        userRoles[1].User.Email.Should().Be("beta@example.com");
        userRoles[2].User.Email.Should().Be("zebra@example.com");
    }

    [Fact]
    public async Task GetUsersWithRoleAsync_ShouldExcludeInactiveUsers()
    {
        // Arrange
        var user1 = User.Create("active@example.com", "hash", "Active", "User");
        var user2 = User.Create("inactive@example.com", "hash", "Inactive", "User");
        
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var activeUserRole = UserRole.Create(user1, _testRole);
        var inactiveUserRole = UserRole.Create(user2, _testRole);
        inactiveUserRole.Deactivate();

        await _userRoleRepository.AssignRoleAsync(activeUserRole);
        await _userRoleRepository.AssignRoleAsync(inactiveUserRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetUsersWithRoleAsync(_testRole.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().User.Email.Should().Be("active@example.com");
    }

    [Fact]
    public async Task GetUsersWithRoleAsync_WithNoUsers_ShouldReturnEmpty()
    {
        // Act
        var result = await _userRoleRepository.GetUsersWithRoleAsync(_testRole.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidUserRole_ShouldPersistChanges()
    {
        // Arrange
        var userRole = UserRole.Create(_testUser, _testRole);
        await _userRoleRepository.AssignRoleAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        userRole.Deactivate();
        await _userRoleRepository.UpdateAsync(userRole);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUserRole = await _context.Set<UserRole>().FindAsync(userRole.Id);
        updatedUserRole!.IsActive.Should().BeFalse();
    }

    #endregion

    #region RemoveRoleAsync Tests

    [Fact]
    public async Task RemoveRoleAsync_WithValidId_ShouldSoftDelete()
    {
        // Arrange
        var userRole = UserRole.Create(_testUser, _testRole);
        await _userRoleRepository.AssignRoleAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        await _userRoleRepository.RemoveRoleAsync(userRole.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deletedUserRole = await _context.Set<UserRole>().IgnoreQueryFilters().FirstOrDefaultAsync(ur => ur.Id == userRole.Id);
        deletedUserRole.Should().NotBeNull();
        deletedUserRole!.IsDeleted.Should().BeTrue();
        
        // Verify it's not returned by GetByIdAsync
        var result = await _userRoleRepository.GetByIdAsync(userRole.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveRoleAsync_WithNonExistentId_ShouldNotThrow()
    {
        // Act
        var action = async () => await _userRoleRepository.RemoveRoleAsync(Guid.NewGuid());

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region UserHasRoleAsync Tests

    [Fact]
    public async Task UserHasRoleAsync_WithActiveRole_ShouldReturnTrue()
    {
        // Arrange
        var userRole = UserRole.Create(_testUser, _testRole);
        await _userRoleRepository.AssignRoleAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.UserHasRoleAsync(_testUser.Id, _testRole.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasRoleAsync_WithInactiveRole_ShouldReturnFalse()
    {
        // Arrange
        var userRole = UserRole.Create(_testUser, _testRole);
        userRole.Deactivate();
        await _userRoleRepository.AssignRoleAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.UserHasRoleAsync(_testUser.Id, _testRole.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasRoleAsync_WithExpiredRole_ShouldReturnFalse()
    {
        // Arrange
        var expiredUserRole = CreateExpiredUserRole(_testUser, _testRole, DateTime.UtcNow.AddDays(-1));
        await _userRoleRepository.AssignRoleAsync(expiredUserRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.UserHasRoleAsync(_testUser.Id, _testRole.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasRoleAsync_WithDeletedRole_ShouldReturnFalse()
    {
        // Arrange
        var userRole = UserRole.Create(_testUser, _testRole);
        userRole.SoftDelete();
        await _userRoleRepository.AssignRoleAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.UserHasRoleAsync(_testUser.Id, _testRole.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasRoleAsync_WithNoRole_ShouldReturnFalse()
    {
        // Act
        var result = await _userRoleRepository.UserHasRoleAsync(_testUser.Id, _testRole.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetExpiredRolesAsync Tests

    [Fact]
    public async Task GetExpiredRolesAsync_WithExpiredRoles_ShouldReturnThem()
    {
        // Arrange
        var activeUserRole = UserRole.Create(_testUser, _testRole, DateTime.UtcNow.AddDays(30));
        var expiredUserRole1 = CreateExpiredUserRole(_testUser, _testRole, DateTime.UtcNow.AddDays(-1));
        var expiredUserRole2 = CreateExpiredUserRole(_testUser, _testRole, DateTime.UtcNow.AddDays(-5));

        await _userRoleRepository.AssignRoleAsync(activeUserRole);
        await _userRoleRepository.AssignRoleAsync(expiredUserRole1);
        await _userRoleRepository.AssignRoleAsync(expiredUserRole2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetExpiredRolesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(ur => ur.Id == expiredUserRole1.Id);
        result.Should().Contain(ur => ur.Id == expiredUserRole2.Id);
    }

    [Fact]
    public async Task GetExpiredRolesAsync_ShouldExcludeInactiveRoles()
    {
        // Arrange
        var expiredActiveUserRole = CreateExpiredUserRole(_testUser, _testRole, DateTime.UtcNow.AddDays(-1));
        var expiredInactiveUserRole = CreateExpiredUserRole(_testUser, _testRole, DateTime.UtcNow.AddDays(-1));
        expiredInactiveUserRole.Deactivate();

        await _userRoleRepository.AssignRoleAsync(expiredActiveUserRole);
        await _userRoleRepository.AssignRoleAsync(expiredInactiveUserRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetExpiredRolesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(expiredActiveUserRole.Id);
    }

    [Fact]
    public async Task GetExpiredRolesAsync_WithNoExpiredRoles_ShouldReturnEmpty()
    {
        // Arrange
        var userRole = UserRole.Create(_testUser, _testRole, DateTime.UtcNow.AddDays(30));
        await _userRoleRepository.AssignRoleAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRoleRepository.GetExpiredRolesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    /// <summary>
    /// Helper method to create an expired user role for testing
    /// Uses reflection to bypass validation in Create method
    /// </summary>
    private static UserRole CreateExpiredUserRole(User user, Role role, DateTime expiresAt)
    {
        var userRole = UserRole.Create(user, role, DateTime.UtcNow.AddDays(1));
        
        // Use reflection to set the ExpiresAt property to a past date
        var expiresAtProperty = typeof(UserRole).GetProperty(nameof(UserRole.ExpiresAt));
        expiresAtProperty!.SetValue(userRole, expiresAt);
        
        return userRole;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

