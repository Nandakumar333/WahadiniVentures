using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Handler for retrieving user's daily login streak information
/// </summary>
public class GetStreakQueryHandler : IRequestHandler<GetStreakQuery, StreakDto>
{
    private readonly IStreakService _streakService;

    public GetStreakQueryHandler(IStreakService streakService)
    {
        _streakService = streakService;
    }

    public async Task<StreakDto> Handle(GetStreakQuery request, CancellationToken cancellationToken)
    {
        return await _streakService.GetUserStreakAsync(request.UserId, cancellationToken);
    }
}
