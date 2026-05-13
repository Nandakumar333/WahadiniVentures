using AutoMapper;
using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Handler for GetAvailableDiscountsQuery using CQRS pattern
/// </summary>
public class GetAvailableDiscountsQueryHandler : IRequestHandler<GetAvailableDiscountsQuery, List<DiscountTypeDto>>
{
    private readonly IDiscountCodeRepository _discountRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetAvailableDiscountsQueryHandler(
        IDiscountCodeRepository discountRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _discountRepository = discountRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<DiscountTypeDto>> Handle(GetAvailableDiscountsQuery request, CancellationToken cancellationToken)
    {
        // Get user to check current points
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");
        }

        // Get all active, non-expired discounts
        var discounts = await _discountRepository.GetActiveCodesAsync(cancellationToken);

        // Map to DTOs and calculate eligibility
        var discountDtos = discounts.Select(d => new DiscountTypeDto
        {
            Id = d.Id,
            Code = d.Code,
            DiscountPercentage = d.DiscountPercentage,
            RequiredPoints = d.RequiredPoints,
            MaxRedemptions = d.MaxRedemptions,
            CurrentRedemptions = d.CurrentRedemptions,
            ExpiryDate = d.ExpiryDate,
            IsActive = d.IsActive,
            CanAfford = user.CurrentPoints >= d.RequiredPoints,
            CanRedeem = d.CanRedeem(user.CurrentPoints)
        }).ToList();

        return discountDtos;
    }
}
