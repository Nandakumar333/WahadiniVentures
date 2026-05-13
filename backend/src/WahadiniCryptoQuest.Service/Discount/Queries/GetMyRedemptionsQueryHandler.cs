using AutoMapper;
using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Common;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Handler for GetMyRedemptionsQuery with pagination support
/// </summary>
public class GetMyRedemptionsQueryHandler : IRequestHandler<GetMyRedemptionsQuery, PaginatedRedemptionsDto>
{
    private readonly IUserDiscountRedemptionRepository _redemptionRepository;
    private readonly IMapper _mapper;

    public GetMyRedemptionsQueryHandler(
        IUserDiscountRedemptionRepository redemptionRepository,
        IMapper mapper)
    {
        _redemptionRepository = redemptionRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedRedemptionsDto> Handle(GetMyRedemptionsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _redemptionRepository.GetUserRedemptionsAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var redemptionDtos = _mapper.Map<List<UserRedemptionDto>>(items);

        var pagination = new PaginationMetadata(
            request.PageNumber,
            request.PageSize,
            totalCount);

        return new PaginatedRedemptionsDto
        {
            Items = redemptionDtos,
            Pagination = pagination
        };
    }
}
