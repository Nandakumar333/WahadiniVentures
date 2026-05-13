using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for Category entity
/// </summary>
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all active categories ordered by display order
    /// </summary>
    public async Task<IEnumerable<Category>> GetActiveCategoriesOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a category by its name
    /// </summary>
    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    /// <summary>
    /// Gets all categories with their course count
    /// </summary>
    public async Task<IEnumerable<CategoryWithCount>> GetWithCourseCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CategoryWithCount
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IconUrl = c.IconUrl ?? string.Empty,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CourseCount = c.Courses.Count(course => course.IsPublished)
            })
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}
