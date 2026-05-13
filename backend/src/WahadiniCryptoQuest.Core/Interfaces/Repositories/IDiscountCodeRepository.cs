using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for DiscountCode entity with discount code operations
    /// </summary>
    public interface IDiscountCodeRepository : IRepository<DiscountCode>
    {
        /// <summary>
        /// Gets a discount code by its unique code string
        /// </summary>
        /// <param name="code">Discount code string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Discount code entity, or null if not found</returns>
       Task<DiscountCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active discount codes (IsActive = true and not expired)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of active discount codes</returns>
       Task<IEnumerable<DiscountCode>> GetActiveCodesAsync(CancellationToken cancellationToken = default);
    }
}
