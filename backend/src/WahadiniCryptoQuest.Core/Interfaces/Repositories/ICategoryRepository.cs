using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Category entity with category-specific operations
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Gets all active categories ordered by display order
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active categories ordered by display order</returns>
    Task<IEnumerable<Category>> GetActiveCategoriesOrderedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its name
    /// </summary>
    /// <param name="name">Category name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category with the specified name, or null if not found</returns>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all categories with their course count
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of categories with course counts</returns>
    Task<IEnumerable<CategoryWithCount>> GetWithCourseCountAsync(CancellationToken cancellationToken = default);
}
