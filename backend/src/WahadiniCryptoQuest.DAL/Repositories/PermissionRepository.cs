using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for Permission entity
/// Provides data access methods for permission management with EF Core
/// Part of DAL layer - implements repository contracts
/// </summary>
public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    private new readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Name == name && !p.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Resource == resource && !p.IsDeleted)
            .OrderBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByActionAsync(string action, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Action == action && !p.IsDeleted)
            .OrderBy(p => p.Resource)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId && rp.IsActive && !rp.IsDeleted)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<Permission> CreateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(permission, cancellationToken);
        return permission;
    }

    public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(permission);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await GetByIdAsync(id, cancellationToken);
        if (permission != null)
        {
            permission.SoftDelete();
            _dbSet.Update(permission);
        }
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(p => p.Name == name && !p.IsDeleted, cancellationToken);
    }

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }
}
