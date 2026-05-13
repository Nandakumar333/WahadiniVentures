using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;

namespace WahadiniCryptoQuest.Service.Commands.Rewards;

/// <summary>
/// Command to process user login and update streak tracking
/// </summary>
public class ProcessStreakCommand : IRequest<StreakDto>
{
    /// <summary>
    /// ID of the user who logged in
    /// </summary>
    public Guid UserId { get; set; }

    public ProcessStreakCommand(Guid userId)
    {
        UserId = userId;
    }
}
