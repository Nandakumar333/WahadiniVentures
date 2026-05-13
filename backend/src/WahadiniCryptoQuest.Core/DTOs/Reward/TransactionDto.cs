namespace WahadiniCryptoQuest.Core.DTOs.Reward;

/// <summary>
/// Individual reward transaction details
/// </summary>
public record TransactionDto
{
    public Guid Id { get; init; }
    public int Amount { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string? ReferenceId { get; init; }
}
