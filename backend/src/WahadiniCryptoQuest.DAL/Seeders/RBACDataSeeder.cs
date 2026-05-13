using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Seeders;

/// <summary>
/// Seeds Role-Based Access Control (RBAC) data into the database
/// Creates default roles (Free, Premium, Admin) with their associated permissions
/// </summary>
public class RBACDataSeeder
{
    private readonly ApplicationDbContext _context;

    public RBACDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds all RBAC data (roles, permissions, role-permission mappings)
    /// Idempotent - safe to run multiple times
    /// </summary>
    public async Task SeedAsync()
    {
        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedRolePermissionsAsync();
    }

    /// <summary>
    /// Seeds all permissions into the database
    /// </summary>
    private async Task SeedPermissionsAsync()
    {
        var permissions = new[]
        {
            // User Management Permissions (Admin only)
            Permission.Create("users", "view", "View user list and details"),
            Permission.Create("users", "create", "Create new users"),
            Permission.Create("users", "edit", "Edit user information"),
            Permission.Create("users", "delete", "Delete users"),

            // Course Permissions
            Permission.Create("courses", "view", "View course list"),
            Permission.Create("courses", "free", "Access free courses"),
            Permission.Create("courses", "premium", "Access premium courses"),
            Permission.Create("courses", "complete", "Mark courses as complete"),
            Permission.Create("courses", "manage", "Full course management"),
            Permission.Create("courses", "create", "Create new courses"),
            Permission.Create("courses", "edit", "Edit courses"),
            Permission.Create("courses", "delete", "Delete courses"),

            // Lesson Permissions
            Permission.Create("lessons", "view", "View lessons"),
            Permission.Create("lessons", "free", "Access free lessons"),
            Permission.Create("lessons", "premium", "Access premium lessons"),
            Permission.Create("lessons", "create", "Create new lessons"),
            Permission.Create("lessons", "edit", "Edit lessons"),
            Permission.Create("lessons", "delete", "Delete lessons"),

            // Task Permissions
            Permission.Create("tasks", "view", "View tasks"),
            Permission.Create("tasks", "submit", "Submit task solutions"),
            Permission.Create("tasks", "create", "Create new tasks"),
            Permission.Create("tasks", "edit", "Edit tasks"),
            Permission.Create("tasks", "delete", "Delete tasks"),
            Permission.Create("tasks", "review", "Review and grade task submissions"),

            // Reward Permissions
            Permission.Create("rewards", "view", "View available rewards"),
            Permission.Create("rewards", "claim", "Claim available rewards"),
            Permission.Create("rewards", "redeem", "Redeem rewards"),
            Permission.Create("rewards", "create", "Create new rewards"),
            Permission.Create("rewards", "edit", "Edit rewards"),
            Permission.Create("rewards", "delete", "Delete rewards"),

            // Subscription Permissions (Admin only)
            Permission.Create("subscriptions", "view", "View subscription details"),
            Permission.Create("subscriptions", "manage", "Manage user subscriptions"),

            // Profile Permissions
            Permission.Create("profile", "read", "Read own profile"),
            Permission.Create("profile", "view", "View own profile"),
            Permission.Create("profile", "edit", "Edit own profile"),

            // Analytics Permissions (Admin only)
            Permission.Create("analytics", "view", "View analytics and reports"),

            // Settings Permissions (Admin only)
            Permission.Create("settings", "manage", "Manage system settings")
        };

        foreach (var permission in permissions)
        {
            var existing = await _context.Set<Permission>()
                .FirstOrDefaultAsync(p => p.Name == permission.Name);

            if (existing == null)
            {
                _context.Set<Permission>().Add(permission);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds default roles into the database
    /// </summary>
    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            Role.Create("Free", "Free tier users with basic access to platform content"),
            Role.Create("Premium", "Premium subscribers with full access to all content and features"),
            Role.Create("Admin", "System administrators with full access to all features and management capabilities"),
            Role.Create("SuperAdmin", "Super administrators with highest privilege level including ability to manage admins")
        };

        foreach (var role in roles)
        {
            var existing = await _context.Set<Role>()
                .FirstOrDefaultAsync(r => r.Name == role.Name);

            if (existing == null)
            {
                _context.Set<Role>().Add(role);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds role-permission mappings
    /// Defines which permissions each role has
    /// </summary>
    private async Task SeedRolePermissionsAsync()
    {
        // Get roles
        var freeRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Free");
        var premiumRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Premium");
        var adminRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Admin");
        var superAdminRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

        if (freeRole == null || premiumRole == null || adminRole == null || superAdminRole == null)
            return;

        // Free role permissions
        var freePermissions = new[]
        {
            "courses:view", "courses:free",
            "lessons:view", "lessons:free",
            "tasks:view", "tasks:submit",
            "rewards:view",
            "profile:read", "profile:view", "profile:edit"
        };

        await AssignPermissionsToRoleAsync(freeRole.Id, freePermissions);

        // Premium role permissions
        var premiumPermissions = new[]
        {
            "courses:view", "courses:free", "courses:premium", "courses:complete",
            "lessons:view", "lessons:free", "lessons:premium",
            "tasks:view", "tasks:submit",
            "rewards:view", "rewards:claim", "rewards:redeem",
            "profile:read", "profile:view", "profile:edit"
        };

        await AssignPermissionsToRoleAsync(premiumRole.Id, premiumPermissions);

        // Admin role permissions (all permissions)
        var adminPermissions = new[]
        {
            // User management
            "users:view", "users:create", "users:edit", "users:delete",
            // Course management
            "courses:view", "courses:free", "courses:premium", "courses:complete",
            "courses:manage", "courses:create", "courses:edit", "courses:delete",
            // Lesson management
            "lessons:view", "lessons:free", "lessons:premium",
            "lessons:create", "lessons:edit", "lessons:delete",
            // Task management
            "tasks:view", "tasks:submit", "tasks:create",
            "tasks:edit", "tasks:delete", "tasks:review",
            // Reward management
            "rewards:view", "rewards:claim", "rewards:redeem", "rewards:create",
            "rewards:edit", "rewards:delete",
            // Subscription management
            "subscriptions:view", "subscriptions:manage",
            // Profile
            "profile:read", "profile:view", "profile:edit",
            // System
            "analytics:view", "settings:manage"
        };

        await AssignPermissionsToRoleAsync(adminRole.Id, adminPermissions);

        // SuperAdmin role permissions (same as Admin for now - hierarchical role checks in authorization policies)
        await AssignPermissionsToRoleAsync(superAdminRole.Id, adminPermissions);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Helper method to assign multiple permissions to a role
    /// </summary>
    private async Task AssignPermissionsToRoleAsync(Guid roleId, string[] permissionNames)
    {
        foreach (var permissionName in permissionNames)
        {
            var permission = await _context.Set<Permission>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == permissionName);

            if (permission == null)
                continue;

            // Check if mapping already exists
            var existingMapping = await _context.Set<RolePermission>()
                .AsNoTracking()
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id);

            if (existingMapping == null)
            {
                var rolePermission = RolePermission.CreateFromIds(roleId, permission.Id);
                _context.Set<RolePermission>().Add(rolePermission);
            }
        }
    }
}
