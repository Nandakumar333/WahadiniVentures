using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

/// <summary>
/// Integration tests for RoleRepository
/// Tests role creation, retrieval, updates, soft deletion, and permission management
/// </summary>
public class RoleRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RoleRepository _roleRepository;

    public RoleRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _roleRepository = new RoleRepository(_context);
    }

    #region CreateAsync and GetByIdAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRole_ShouldPersistToDatabase()
    {
        // Arrange
        var role = Role.Create("Premium", "Premium tier role with advanced features");

        // Act
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        var savedRole = await _context.Roles.FindAsync(role.Id);
        savedRole.Should().NotBeNull();
        savedRole!.Name.Should().Be("Premium");
        savedRole.Description.Should().Be("Premium tier role with advanced features");
        savedRole.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnRole()
    {
        // Arrange
        var role = Role.Create("Free", "Free tier role");
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetByIdAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(role.Id);
        result.Name.Should().Be("Free");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _roleRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedRole_ShouldReturnNull()
    {
        // Arrange
        var role = Role.Create("Deleted", "This role will be deleted");
        role.SoftDelete();
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetByIdAsync(role.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_WithValidName_ShouldReturnRole()
    {
        // Arrange
        var role = Role.Create("Admin", "Administrator role");
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetByNameAsync("Admin");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Admin");
        result.Description.Should().Be("Administrator role");
    }

    [Fact]
    public async Task GetByNameAsync_WithNonExistentName_ShouldReturnNull()
    {
        // Act
        var result = await _roleRepository.GetByNameAsync("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_WithDeletedRole_ShouldReturnNull()
    {
        // Arrange
        var role = Role.Create("DeletedRole", "This role is soft deleted");
        role.SoftDelete();
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetByNameAsync("DeletedRole");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetActiveRolesAsync Tests

    [Fact]
    public async Task GetActiveRolesAsync_WithMultipleRoles_ShouldReturnAllActiveOrderedByName()
    {
        // Arrange
        var role1 = Role.Create("Premium", "Premium role");
        var role2 = Role.Create("Admin", "Admin role");
        var role3 = Role.Create("Free", "Free role");
        var deletedRole = Role.Create("Deleted", "Deleted role");
        deletedRole.SoftDelete();

        await _roleRepository.CreateAsync(role1);
        await _roleRepository.CreateAsync(role2);
        await _roleRepository.CreateAsync(role3);
        await _roleRepository.CreateAsync(deletedRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetActiveRolesAsync();

        // Assert
        var roles = result.ToList();
        roles.Should().HaveCount(3);
        roles[0].Name.Should().Be("Admin"); // Alphabetically first
        roles[1].Name.Should().Be("Free");
        roles[2].Name.Should().Be("Premium");
    }

    [Fact]
    public async Task GetActiveRolesAsync_WithNoRoles_ShouldReturnEmpty()
    {
        // Act
        var result = await _roleRepository.GetActiveRolesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveRolesAsync_WithOnlyDeletedRoles_ShouldReturnEmpty()
    {
        // Arrange
        var role = Role.Create("DeletedRole", "This is deleted");
        role.SoftDelete();
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetActiveRolesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedRolesOrderedByName()
    {
        // Arrange
        var role1 = Role.Create("Zebra", "Z role");
        var role2 = Role.Create("Alpha", "A role");
        var deletedRole = Role.Create("Deleted", "D role");
        deletedRole.SoftDelete();

        await _roleRepository.CreateAsync(role1);
        await _roleRepository.CreateAsync(role2);
        await _roleRepository.CreateAsync(deletedRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetAllAsync();

        // Assert
        var roles = result.ToList();
        roles.Should().HaveCount(2);
        roles[0].Name.Should().Be("Alpha");
        roles[1].Name.Should().Be("Zebra");
    }

    #endregion

    #region GetRoleWithPermissionsAsync Tests

    [Fact]
    public async Task GetRoleWithPermissionsAsync_WithValidId_ShouldIncludePermissions()
    {
        // Arrange
        var role = Role.Create("TestRole", "Test role with permissions");
        var permission1 = Permission.Create("read:data", "Read data permission");
        var permission2 = Permission.Create("write:data", "Write data permission");
        
        role.AddPermission(permission1);
        role.AddPermission(permission2);

        await _context.Roles.AddAsync(role);
        await _context.Permissions.AddAsync(permission1);
        await _context.Permissions.AddAsync(permission2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetRoleWithPermissionsAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.RolePermissions.Should().HaveCount(2);
        result.RolePermissions.Should().OnlyContain(rp => rp.Permission != null);
    }

    [Fact]
    public async Task GetRoleWithPermissionsAsync_WithNoPermissions_ShouldReturnRoleWithEmptyPermissions()
    {
        // Arrange
        var role = Role.Create("NoPermissions", "Role without permissions");
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetRoleWithPermissionsAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.RolePermissions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRoleWithPermissionsAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _roleRepository.GetRoleWithPermissionsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoleWithPermissionsAsync_WithDeletedRole_ShouldReturnNull()
    {
        // Arrange
        var role = Role.Create("DeletedWithPerms", "Deleted role");
        var permission = Permission.Create("test:perm", "Test permission");
        role.AddPermission(permission);
        role.SoftDelete();

        await _context.Roles.AddAsync(role);
        await _context.Permissions.AddAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.GetRoleWithPermissionsAsync(role.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidRole_ShouldPersistChanges()
    {
        // Arrange
        var role = Role.Create("Original", "Original description");
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        role.UpdateDescription("Updated description");
        await _roleRepository.UpdateAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        var updatedRole = await _context.Roles.FindAsync(role.Id);
        updatedRole!.Description.Should().Be("Updated description");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldSoftDeleteRole()
    {
        // Arrange
        var role = Role.Create("ToDelete", "Role to be deleted");
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        await _roleRepository.DeleteAsync(role.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deletedRole = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == role.Id);
        deletedRole.Should().NotBeNull();
        deletedRole!.IsDeleted.Should().BeTrue();
        
        // Verify it's not returned by GetByIdAsync
        var result = await _roleRepository.GetByIdAsync(role.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldNotThrow()
    {
        // Act
        var action = async () => await _roleRepository.DeleteAsync(Guid.NewGuid());

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingRole_ShouldReturnTrue()
    {
        // Arrange
        var role = Role.Create("Existing", "Existing role");
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.ExistsAsync("Existing");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentRole_ShouldReturnFalse()
    {
        // Act
        var result = await _roleRepository.ExistsAsync("NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithDeletedRole_ShouldReturnFalse()
    {
        // Arrange
        var role = Role.Create("DeletedCheck", "Deleted role");
        role.SoftDelete();
        await _roleRepository.CreateAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleRepository.ExistsAsync("DeletedCheck");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

