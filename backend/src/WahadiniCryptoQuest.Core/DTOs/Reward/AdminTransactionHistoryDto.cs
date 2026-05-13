namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// Extended transaction DTO with admin audit information
/// </summary>
public record AdminTransactionHistoryDto : TransactionDto
{
    /// <summary>
    /// Admin user who created the transaction (for manual adjustments)
    /// </summary>
    public Guid? AdminUserId { get; init; }

    /// <summary>
    /// Full name of the admin who created the transaction
    /// </summary>
    public string? AdminName { get; init; }

    /// <summary>
    /// Indicates if this was an admin action
    /// </summary>
    public bool IsAdminAction => AdminUserId.HasValue;
}
