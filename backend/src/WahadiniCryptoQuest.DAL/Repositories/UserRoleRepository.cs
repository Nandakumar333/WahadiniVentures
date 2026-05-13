using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for UserRole entity
/// Provides data access methods for user-role assignments with EF Core
/// Part of DAL layer - implements repository contracts
/// </summary>
public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    private new readonly ApplicationDbContext _context;

    public UserRoleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId && !ur.IsDeleted)
            .OrderByDescending(ur => ur.AssignedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserRole?> GetActiveUserRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId 
                      && ur.IsActive 
                      && !ur.IsDeleted
                      && (ur.ExpiresAt == null || ur.ExpiresAt > now))
            .OrderByDescending(ur => ur.AssignedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserRole>> GetUsersWithRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ur => ur.User)
            .Where(ur => ur.RoleId == roleId && ur.IsActive && !ur.IsDeleted)
            .OrderBy(ur => ur.User.Email)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserRole> AssignRoleAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(userRole, cancellationToken);
        return userRole;
    }

    public async Task UpdateAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(userRole);
        await Task.CompletedTask;
    }

    public async Task RemoveRoleAsync(Guid userRoleId, CancellationToken cancellationToken = default)
    {
        var userRole = await GetByIdAsync(userRoleId, cancellationToken);
        if (userRole != null)
        {
            userRole.SoftDelete();
            _dbSet.Update(userRole);
        }
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .AnyAsync(ur => ur.UserId == userId 
                         && ur.RoleId == roleId 
                         && ur.IsActive 
                         && !ur.IsDeleted
                         && (ur.ExpiresAt == null || ur.ExpiresAt > now), 
                      cancellationToken);
    }

    public async Task<IEnumerable<UserRole>> GetExpiredRolesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(ur => ur.IsActive 
                      && !ur.IsDeleted 
                      && ur.ExpiresAt != null 
                      && ur.ExpiresAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ur => ur.Role)
            .Include(ur => ur.User)
            .FirstOrDefaultAsync(ur => ur.Id == id && !ur.IsDeleted, cancellationToken);
    }
}
