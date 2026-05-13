using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for user operations
/// Provides contract for user data access and management
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists with the given email
    /// </summary>
    /// <param name="email">Email address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID including their roles
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User with roles if found, null otherwise</returns>
    Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by role type
    /// </summary>
    /// <param name="role">Role type to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of users with the specified role</returns>
    Task<IEnumerable<User>> GetByRoleAsync(Enums.UserRoleEnum role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active users (IsActive = true)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active users</returns>
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the user's last login timestamp
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their referral code
    /// </summary>
    /// <param name="referralCode">Referral code to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User if found, null otherwise</returns>
    Task<User?> GetByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets queryable for complex queries (e.g., leaderboard calculations)
    /// </summary>
    /// <returns>IQueryable for users</returns>
    IQueryable<User> GetQueryable();
}
