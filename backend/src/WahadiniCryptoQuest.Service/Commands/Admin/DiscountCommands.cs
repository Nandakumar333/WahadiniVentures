using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to create a discount code
/// T135: US5 - Reward System Management
/// </summary>
public class CreateDiscountCodeCommand : IRequest<Guid>
{
    public CreateDiscountCodeDto Data { get; set; } = null!;
    public Guid AdminUserId { get; set; }
}

/// <summary>
/// Command to manually adjust user points
/// T139: US5 - Reward System Management
/// </summary>
public class AdjustPointsCommand : IRequest<int>
{
    public Guid UserId { get; set; }
    public int AdjustmentAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid AdminUserId { get; set; }
}
