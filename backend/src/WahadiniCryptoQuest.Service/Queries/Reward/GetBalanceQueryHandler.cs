using AutoMapper;
using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Handler for retrieving user's reward point balance
/// </summary>
public class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, BalanceDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetBalanceQueryHandler(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<BalanceDto> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");

        // Map User entity to BalanceDto
        return _mapper.Map<BalanceDto>(user);
    }
}
