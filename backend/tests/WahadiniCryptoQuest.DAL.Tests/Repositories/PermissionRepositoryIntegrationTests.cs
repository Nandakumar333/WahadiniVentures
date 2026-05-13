using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.DAL.Context;
using WahadiniCryptoQuest.DAL.Repositories;
using Xunit;

namespace WahadiniCryptoQuest.DAL.Tests.Repositories;

/// <summary>
/// Integration tests for PermissionRepository
/// Tests permission creation, retrieval by various criteria, updates, soft deletion, and role associations
/// </summary>
public class PermissionRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PermissionRepository _permissionRepository;

    public PermissionRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _permissionRepository = new PermissionRepository(_context);
    }

    #region CreateAsync and GetByIdAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidPermission_ShouldPersistToDatabase()
    {
        // Arrange
        var permission = Permission.Create("courses", "create", "Create new courses");

        // Act
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Assert
        var savedPermission = await _context.Permissions.FindAsync(permission.Id);
        savedPermission.Should().NotBeNull();
        savedPermission!.Name.Should().Be("courses:create");
        savedPermission.Resource.Should().Be("courses");
        savedPermission.Action.Should().Be("create");
        savedPermission.Description.Should().Be("Create new courses");
        savedPermission.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnPermission()
    {
        // Arrange
        var permission = Permission.Create("tasks", "review", "Review submitted tasks");
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByIdAsync(permission.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(permission.Id);
        result.Name.Should().Be("tasks:review");
        result.Resource.Should().Be("tasks");
        result.Action.Should().Be("review");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _permissionRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedPermission_ShouldReturnNull()
    {
        // Arrange
        var permission = Permission.Create("users", "delete", "Delete users");
        permission.SoftDelete();
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByIdAsync(permission.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_WithValidName_ShouldReturnPermission()
    {
        // Arrange
        var permission = Permission.Create("reports", "generate", "Generate reports");
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByNameAsync("reports:generate");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("reports:generate");
        result.Description.Should().Be("Generate reports");
    }

    [Fact]
    public async Task GetByNameAsync_WithNonExistentName_ShouldReturnNull()
    {
        // Act
        var result = await _permissionRepository.GetByNameAsync("nonexistent:permission");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_WithDeletedPermission_ShouldReturnNull()
    {
        // Arrange
        var permission = Permission.Create("settings", "modify", "Modify settings");
        permission.SoftDelete();
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByNameAsync("settings:modify");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByResourceAsync Tests

    [Fact]
    public async Task GetByResourceAsync_WithMultiplePermissions_ShouldReturnOrderedByAction()
    {
        // Arrange
        var perm1 = Permission.Create("courses", "update", "Update courses");
        var perm2 = Permission.Create("courses", "create", "Create courses");
        var perm3 = Permission.Create("courses", "delete", "Delete courses");
        var perm4 = Permission.Create("tasks", "create", "Create tasks"); // Different resource
        var deletedPerm = Permission.Create("courses", "archive", "Archive courses");
        deletedPerm.SoftDelete();

        await _permissionRepository.CreateAsync(perm1);
        await _permissionRepository.CreateAsync(perm2);
        await _permissionRepository.CreateAsync(perm3);
        await _permissionRepository.CreateAsync(perm4);
        await _permissionRepository.CreateAsync(deletedPerm);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByResourceAsync("courses");

        // Assert
        var permissions = result.ToList();
        permissions.Should().HaveCount(3);
        permissions[0].Action.Should().Be("create"); // Alphabetically first
        permissions[1].Action.Should().Be("delete");
        permissions[2].Action.Should().Be("update");
        permissions.Should().OnlyContain(p => p.Resource == "courses");
    }

    [Fact]
    public async Task GetByResourceAsync_WithNoMatchingResource_ShouldReturnEmpty()
    {
        // Arrange
        var permission = Permission.Create("courses", "read", "Read courses");
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByResourceAsync("tasks");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByResourceAsync_WithOnlyDeletedPermissions_ShouldReturnEmpty()
    {
        // Arrange
        var permission = Permission.Create("users", "read", "Read users");
        permission.SoftDelete();
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByResourceAsync("users");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByActionAsync Tests

    [Fact]
    public async Task GetByActionAsync_WithMultiplePermissions_ShouldReturnOrderedByResource()
    {
        // Arrange
        var perm1 = Permission.Create("tasks", "create", "Create tasks");
        var perm2 = Permission.Create("courses", "create", "Create courses");
        var perm3 = Permission.Create("users", "create", "Create users");
        var perm4 = Permission.Create("courses", "update", "Update courses"); // Different action
        var deletedPerm = Permission.Create("reports", "create", "Create reports");
        deletedPerm.SoftDelete();

        await _permissionRepository.CreateAsync(perm1);
        await _permissionRepository.CreateAsync(perm2);
        await _permissionRepository.CreateAsync(perm3);
        await _permissionRepository.CreateAsync(perm4);
        await _permissionRepository.CreateAsync(deletedPerm);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByActionAsync("create");

        // Assert
        var permissions = result.ToList();
        permissions.Should().HaveCount(3);
        permissions[0].Resource.Should().Be("courses"); // Alphabetically first
        permissions[1].Resource.Should().Be("tasks");
        permissions[2].Resource.Should().Be("users");
        permissions.Should().OnlyContain(p => p.Action == "create");
    }

    [Fact]
    public async Task GetByActionAsync_WithNoMatchingAction_ShouldReturnEmpty()
    {
        // Arrange
        var permission = Permission.Create("courses", "read", "Read courses");
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByActionAsync("delete");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByActionAsync_WithOnlyDeletedPermissions_ShouldReturnEmpty()
    {
        // Arrange
        var permission = Permission.Create("courses", "review", "Review courses");
        permission.SoftDelete();
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetByActionAsync("review");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetPermissionsByRoleIdAsync Tests

    [Fact]
    public async Task GetPermissionsByRoleIdAsync_WithMultipleActivePermissions_ShouldReturnOrderedByResourceThenAction()
    {
        // Arrange
        var role = Role.Create("Admin", "Administrator role");
        var perm1 = Permission.Create("users", "delete", "Delete users");
        var perm2 = Permission.Create("courses", "update", "Update courses");
        var perm3 = Permission.Create("courses", "create", "Create courses");
        var perm4 = Permission.Create("tasks", "review", "Review tasks");
        var perm5 = Permission.Create("reports", "generate", "Generate reports");
        
        var rolePerm1 = RolePermission.Create(role, perm1);
        var rolePerm2 = RolePermission.Create(role, perm2);
        var rolePerm3 = RolePermission.Create(role, perm3);
        var rolePerm4 = RolePermission.Create(role, perm4);

        // Add an inactive role permission
        var inactiveRolePerm = RolePermission.Create(role, perm5);
        inactiveRolePerm.Deactivate();

        await _context.Roles.AddAsync(role);
        await _context.Permissions.AddRangeAsync(perm1, perm2, perm3, perm4, perm5);
        await _context.Set<RolePermission>().AddRangeAsync(rolePerm1, rolePerm2, rolePerm3, rolePerm4, inactiveRolePerm);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetPermissionsByRoleIdAsync(role.Id);

        // Assert
        var permissions = result.ToList();
        permissions.Should().HaveCount(4);
        permissions[0].Resource.Should().Be("courses"); // First resource alphabetically
        permissions[0].Action.Should().Be("create"); // First action for courses
        permissions[1].Resource.Should().Be("courses");
        permissions[1].Action.Should().Be("update"); // Second action for courses
        permissions[2].Resource.Should().Be("tasks");
        permissions[3].Resource.Should().Be("users");
    }

    [Fact]
    public async Task GetPermissionsByRoleIdAsync_WithNoPermissions_ShouldReturnEmpty()
    {
        // Arrange
        var role = Role.Create("EmptyRole", "Role without permissions");
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetPermissionsByRoleIdAsync(role.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPermissionsByRoleIdAsync_WithDeletedRolePermission_ShouldExcludeIt()
    {
        // Arrange
        var role = Role.Create("TestRole", "Test role");
        var perm1 = Permission.Create("courses", "read", "Read courses");
        var perm2 = Permission.Create("tasks", "read", "Read tasks");
        
        var rolePerm1 = RolePermission.Create(role, perm1);
        var rolePerm2 = RolePermission.Create(role, perm2);
        rolePerm2.SoftDelete(); // Soft delete the role permission

        await _context.Roles.AddAsync(role);
        await _context.Permissions.AddRangeAsync(perm1, perm2);
        await _context.Set<RolePermission>().AddRangeAsync(rolePerm1, rolePerm2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetPermissionsByRoleIdAsync(role.Id);

        // Assert
        var permissions = result.ToList();
        permissions.Should().HaveCount(1);
        permissions[0].Name.Should().Be("courses:read");
    }

    [Fact]
    public async Task GetPermissionsByRoleIdAsync_WithDeletedPermission_ShouldExcludeIt()
    {
        // Arrange
        var role = Role.Create("TestRole", "Test role");
        var perm1 = Permission.Create("courses", "read", "Read courses");
        var perm2 = Permission.Create("tasks", "read", "Read tasks");
        perm2.SoftDelete(); // Soft delete the permission
        
        var rolePerm1 = RolePermission.Create(role, perm1);
        var rolePerm2 = RolePermission.Create(role, perm2);

        await _context.Roles.AddAsync(role);
        await _context.Permissions.AddRangeAsync(perm1, perm2);
        await _context.Set<RolePermission>().AddRangeAsync(rolePerm1, rolePerm2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetPermissionsByRoleIdAsync(role.Id);

        // Assert
        var permissions = result.ToList();
        permissions.Should().HaveCount(1);
        permissions[0].Name.Should().Be("courses:read");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidPermission_ShouldPersistChanges()
    {
        // Arrange
        var permission = Permission.Create("courses", "manage", "Original description");
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        permission.UpdateDescription("Updated description for course management");
        await _permissionRepository.UpdateAsync(permission);
        await _context.SaveChangesAsync();

        // Assert
        var updatedPermission = await _context.Permissions.FindAsync(permission.Id);
        updatedPermission!.Description.Should().Be("Updated description for course management");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldSoftDeletePermission()
    {
        // Arrange
        var permission = Permission.Create("temp", "test", "Temporary test permission");
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        await _permissionRepository.DeleteAsync(permission.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deletedPermission = await _context.Permissions.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == permission.Id);
        deletedPermission.Should().NotBeNull();
        deletedPermission!.IsDeleted.Should().BeTrue();
        
        // Verify it's not returned by GetByIdAsync
        var result = await _permissionRepository.GetByIdAsync(permission.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldNotThrow()
    {
        // Act
        var action = async () => await _permissionRepository.DeleteAsync(Guid.NewGuid());

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingPermission_ShouldReturnTrue()
    {
        // Arrange
        var permission = Permission.Create("analytics", "view", "View analytics");
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.ExistsAsync("analytics:view");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentPermission_ShouldReturnFalse()
    {
        // Act
        var result = await _permissionRepository.ExistsAsync("nonexistent:permission");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithDeletedPermission_ShouldReturnFalse()
    {
        // Arrange
        var permission = Permission.Create("audit", "access", "Access audit logs");
        permission.SoftDelete();
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.ExistsAsync("audit:access");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithMultiplePermissions_ShouldReturnOrderedByResourceThenAction()
    {
        // Arrange
        var perm1 = Permission.Create("users", "delete", "Delete users");
        var perm2 = Permission.Create("courses", "update", "Update courses");
        var perm3 = Permission.Create("courses", "create", "Create courses");
        var perm4 = Permission.Create("tasks", "review", "Review tasks");
        var deletedPerm = Permission.Create("admin", "access", "Admin access");
        deletedPerm.SoftDelete();

        await _permissionRepository.CreateAsync(perm1);
        await _permissionRepository.CreateAsync(perm2);
        await _permissionRepository.CreateAsync(perm3);
        await _permissionRepository.CreateAsync(perm4);
        await _permissionRepository.CreateAsync(deletedPerm);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetAllAsync();

        // Assert
        var permissions = result.ToList();
        permissions.Should().HaveCount(4);
        permissions[0].Resource.Should().Be("courses");
        permissions[0].Action.Should().Be("create"); // First action for courses
        permissions[1].Resource.Should().Be("courses");
        permissions[1].Action.Should().Be("update"); // Second action for courses
        permissions[2].Resource.Should().Be("tasks");
        permissions[3].Resource.Should().Be("users");
    }

    [Fact]
    public async Task GetAllAsync_WithNoPermissions_ShouldReturnEmpty()
    {
        // Act
        var result = await _permissionRepository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithOnlyDeletedPermissions_ShouldReturnEmpty()
    {
        // Arrange
        var permission = Permission.Create("deleted", "test", "Deleted test");
        permission.SoftDelete();
        await _permissionRepository.CreateAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionRepository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

