using System.ComponentModel.DataAnnotations;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Immutable ledger for all reward point transactions
/// Event-sourced append-only transaction log with full audit trail
/// </summary>
public class RewardTransaction : BaseEntity
{
    public Guid UserId { get; set; }

    public int Amount { get; set; } // Can be negative for redemptions/penalties

    public TransactionType TransactionType { get; set; }

    [MaxLength(100)]
    public string? ReferenceId { get; set; }

    [MaxLength(50)]
    public string? ReferenceType { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Admin user who created manual adjustment (Bonus/Penalty only)
    /// </summary>
    public Guid? AdminUserId { get; set; }

    /// <summary>
    /// Snapshot of user's balance after this transaction (denormalized for auditing)
    /// </summary>
    public int BalanceAfter { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual User? AdminUser { get; set; }

    // Factory method
    public static RewardTransaction Create(
        Guid userId,
        int amount,
        TransactionType type,
        string description,
        int balanceAfter,
        string? referenceId = null,
        string? referenceType = null,
        Guid? adminUserId = null)
    {
        if (amount == 0)
            throw new ArgumentException("Transaction amount cannot be zero");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description required");

        return new RewardTransaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = type,
            Description = description,
            BalanceAfter = balanceAfter,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            AdminUserId = adminUserId
        };
    }
}
