using System.Linq.Expressions;

namespace WahadiniCryptoQuest.Core.Common;

/// <summary>
/// Generic repository interface for common data access operations
/// Provides standard CRUD operations for all entities
/// </summary>
/// <typeparam name="T">Entity type that implements IEntity</typeparam>
public interface IRepository<T> where T : class, IEntity
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>List of all entities</returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// Finds entities based on a predicate
    /// </summary>
    /// <param name="predicate">Search condition</param>
    /// <returns>List of matching entities</returns>
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Gets the first entity that matches the predicate
    /// </summary>
    /// <param name="predicate">Search condition</param>
    /// <returns>First matching entity or null</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Checks if any entity matches the predicate
    /// </summary>
    /// <param name="predicate">Search condition</param>
    /// <returns>True if any entity matches, false otherwise</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Counts entities that match the predicate
    /// </summary>
    /// <param name="predicate">Search condition</param>
    /// <returns>Number of matching entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Gets entities with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of entities</returns>
    Task<List<T>> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Gets entities with pagination and filtering
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="orderBy">Optional ordering function</param>
    /// <returns>Paginated and filtered list of entities</returns>
    Task<List<T>> GetPagedAsync(
        Expression<Func<T, bool>> predicate,
        int pageNumber,
        int pageSize,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <returns>Added entity</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">Entities to add</param>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <returns>Updated entity</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by ID
    /// </summary>
    /// <param name="id">ID of entity to delete</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Deletes multiple entities
    /// </summary>
    /// <param name="entities">Entities to delete</param>
    Task DeleteRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Performs soft delete on an entity (if supported)
    /// </summary>
    /// <param name="id">ID of entity to soft delete</param>
    Task SoftDeleteAsync(Guid id);

    // Note: SaveChangesAsync is handled by IUnitOfWork, not individual repositories
}