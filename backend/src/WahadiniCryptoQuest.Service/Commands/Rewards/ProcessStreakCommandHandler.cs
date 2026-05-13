using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Commands.Rewards;

/// <summary>
/// Handler for processing user login and updating streak tracking
/// </summary>
public class ProcessStreakCommandHandler : IRequestHandler<ProcessStreakCommand, StreakDto>
{
    private readonly IStreakService _streakService;

    public ProcessStreakCommandHandler(IStreakService streakService)
    {
        _streakService = streakService;
    }

    public async Task<StreakDto> Handle(ProcessStreakCommand request, CancellationToken cancellationToken)
    {
        return await _streakService.ProcessLoginAsync(request.UserId, cancellationToken);
    }
}
