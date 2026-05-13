using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Services;
using Xunit;

namespace WahadiniCryptoQuest.Service.Tests.Services;

/// <summary>
/// Unit tests for AuthorizationService
/// Tests authorization business logic, permission checking, role validation, and caching
/// </summary>
public class AuthorizationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<IPermissionRepository> _mockPermissionRepository;
    private readonly Mock<IUserRoleRepository> _mockUserRoleRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<AuthorizationService>> _mockLogger;
    private readonly AuthorizationService _authorizationService;

    public AuthorizationServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockPermissionRepository = new Mock<IPermissionRepository>();
        _mockUserRoleRepository = new Mock<IUserRoleRepository>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<AuthorizationService>>();

        _authorizationService = new AuthorizationService(
            _mockUserRepository.Object,
            _mockRoleRepository.Object,
            _mockPermissionRepository.Object,
            _mockUserRoleRepository.Object,
            _memoryCache,
            _mockLogger.Object
        );
    }

    #region HasPermissionAsync Tests

    [Fact]
    public async Task HasPermissionAsync_WithUserHavingPermission_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = Core.Entities.UserRole.Create(user, role);
        var permission = Permission.Create("courses", "complete");

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUserRoleRepository
            .Setup(r => r.GetActiveUserRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

        _mockPermissionRepository
            .Setup(r => r.GetPermissionsByRoleIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { permission });

        // Act
        var result = await _authorizationService.HasPermissionAsync(userId, "courses:complete");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WithUserNotHavingPermission_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Free", "Free users");
        var userRole = Core.Entities.UserRole.Create(user, role);
        var permission = Permission.Create("courses", "view");

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUserRoleRepository
            .Setup(r => r.GetActiveUserRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

        _mockPermissionRepository
            .Setup(r => r.GetPermissionsByRoleIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { permission });

        // Act
        var result = await _authorizationService.HasPermissionAsync(userId, "courses:complete");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authorizationService.HasPermissionAsync(userId, "courses:complete");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasRoleAsync Tests

    [Fact]
    public async Task HasRoleAsync_WithUserHavingRole_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        user.UpgradeRole(UserRoleEnum.Premium);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authorizationService.HasRoleAsync(userId, UserRoleEnum.Premium);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasRoleAsync_WithUserNotHavingRole_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        // User defaults to Free role

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authorizationService.HasRoleAsync(userId, UserRoleEnum.Premium);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasRoleAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authorizationService.HasRoleAsync(userId, UserRoleEnum.Premium);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasAnyRoleAsync Tests

    [Fact]
    public async Task HasAnyRoleAsync_WithUserHavingOneOfRoles_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        user.UpgradeRole(UserRoleEnum.Premium);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authorizationService.HasAnyRoleAsync(
            userId, 
            new[] { UserRoleEnum.Premium, UserRoleEnum.Admin }
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnyRoleAsync_WithUserNotHavingAnyRole_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        // User defaults to Free role

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authorizationService.HasAnyRoleAsync(
            userId,
            new[] { UserRoleEnum.Premium, UserRoleEnum.Admin }
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyRoleAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authorizationService.HasAnyRoleAsync(
            userId,
            new[] { UserRoleEnum.Premium, UserRoleEnum.Admin }
        );

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsSubscriptionActiveAsync Tests

    [Fact]
    public async Task IsSubscriptionActiveAsync_WithPremiumUserRequiringPremium_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        user.UpgradeRole(UserRoleEnum.Premium);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authorizationService.IsSubscriptionActiveAsync(userId, UserRoleEnum.Premium);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSubscriptionActiveAsync_WithFreeUserRequiringPremium_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        // User defaults to Free role

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authorizationService.IsSubscriptionActiveAsync(userId, UserRoleEnum.Premium);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSubscriptionActiveAsync_WithAdminUserRequiringPremium_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        user.UpgradeRole(UserRoleEnum.Admin);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authorizationService.IsSubscriptionActiveAsync(userId, UserRoleEnum.Premium);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSubscriptionActiveAsync_WithPremiumUserRequiringFree_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        user.UpgradeRole(UserRoleEnum.Premium);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authorizationService.IsSubscriptionActiveAsync(userId, UserRoleEnum.Free);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSubscriptionActiveAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authorizationService.IsSubscriptionActiveAsync(userId, UserRoleEnum.Premium);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetUserRoleAsync Tests

    [Fact]
    public async Task GetUserRoleAsync_WithExistingUser_ShouldReturnUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        user.UpgradeRole(UserRoleEnum.Premium);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authorizationService.GetUserRoleAsync(userId);

        // Assert
        result.Should().Be(UserRoleEnum.Premium);
    }

    [Fact]
    public async Task GetUserRoleAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authorizationService.GetUserRoleAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserRoleAsync_ShouldCacheResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        user.UpgradeRole(UserRoleEnum.Premium);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result1 = await _authorizationService.GetUserRoleAsync(userId);
        var result2 = await _authorizationService.GetUserRoleAsync(userId);

        // Assert
        result1.Should().Be(UserRoleEnum.Premium);
        result2.Should().Be(UserRoleEnum.Premium);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once); // Should only call once due to caching
    }

    #endregion

    #region GetUserPermissionsAsync Tests

    [Fact]
    public async Task GetUserPermissionsAsync_WithUserHavingPermissions_ShouldReturnPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = Core.Entities.UserRole.Create(user, role);
        var permissions = new[]
        {
            Permission.Create("courses", "complete"),
            Permission.Create("tasks", "submit"),
            Permission.Create("rewards", "claim")
        };

        _mockUserRoleRepository
            .Setup(r => r.GetActiveUserRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

        _mockPermissionRepository
            .Setup(r => r.GetPermissionsByRoleIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result = await _authorizationService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("courses:complete");
        result.Should().Contain("tasks:submit");
        result.Should().Contain("rewards:claim");
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithUserHavingNoActiveRole_ShouldReturnEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRoleRepository
            .Setup(r => r.GetActiveUserRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Core.Entities.UserRole?)null);

        // Act
        var result = await _authorizationService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ShouldCacheResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = Core.Entities.UserRole.Create(user, role);
        var permissions = new[] { Permission.Create("courses", "complete") };

        _mockUserRoleRepository
            .Setup(r => r.GetActiveUserRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

        _mockPermissionRepository
            .Setup(r => r.GetPermissionsByRoleIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result1 = await _authorizationService.GetUserPermissionsAsync(userId);
        var result2 = await _authorizationService.GetUserPermissionsAsync(userId);

        // Assert
        result1.Should().HaveCount(1);
        result2.Should().HaveCount(1);
        _mockUserRoleRepository.Verify(
            r => r.GetActiveUserRoleAsync(userId, It.IsAny<CancellationToken>()), 
            Times.Once
        ); // Should only call once due to caching
    }

    #endregion

    #region InvalidateUserPermissionsCache Tests

    [Fact]
    public async Task InvalidateUserPermissionsCache_ShouldRemoveCachedPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User", "password123");
        var role = Role.Create("Premium", "Premium users");
        var userRole = Core.Entities.UserRole.Create(user, role);
        var permissions = new[] { Permission.Create("courses", "complete") };

        _mockUserRoleRepository
            .Setup(r => r.GetActiveUserRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

        _mockPermissionRepository
            .Setup(r => r.GetPermissionsByRoleIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act - Call twice to cache
        await _authorizationService.GetUserPermissionsAsync(userId);
        
        // Invalidate cache
        _authorizationService.InvalidateUserPermissionsCache(userId);
        
        // Call again after invalidation
        await _authorizationService.GetUserPermissionsAsync(userId);

        // Assert
        _mockUserRoleRepository.Verify(
            r => r.GetActiveUserRoleAsync(userId, It.IsAny<CancellationToken>()),
            Times.Exactly(2) // Should call twice: once before invalidation, once after
        );
    }

    #endregion
}
