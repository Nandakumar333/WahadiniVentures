using MediatR;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Service.Commands.Rewards;

public record AwardPointsCommand(
    Guid UserId,
    int Amount,
    TransactionType Type,
    string Description,
    string? ReferenceId = null,
    string? ReferenceType = null,
    Guid? AdminUserId = null
) : IRequest<Guid>;
