using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Queries.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for getting discount codes with status calculation
/// T134: US5 - Reward System Management
/// </summary>
public class GetDiscountCodesQueryHandler : IRequestHandler<GetDiscountCodesQuery, List<DiscountCodeDto>>
{
    private readonly IDiscountCodeRepository _discountCodeRepository;

    public GetDiscountCodesQueryHandler(IDiscountCodeRepository discountCodeRepository)
    {
        _discountCodeRepository = discountCodeRepository;
    }

    public async Task<List<DiscountCodeDto>> Handle(GetDiscountCodesQuery request, CancellationToken cancellationToken)
    {
        var discountCodes = await _discountCodeRepository.GetAllAsync();
        var now = DateTime.UtcNow;

        var dtos = discountCodes.Select(dc =>
        {
            string status;
            if (dc.ExpiryDate.HasValue && dc.ExpiryDate.Value < now)
            {
                status = "Expired";
            }
            else if (dc.UsageLimit > 0 && dc.UsageCount >= dc.UsageLimit)
            {
                status = "FullyRedeemed";
            }
            else if (!dc.IsActive)
            {
                status = "Inactive";
            }
            else
            {
                status = "Active";
            }

            return new DiscountCodeDto
            {
                Id = dc.Id,
                Code = dc.Code,
                DiscountPercentage = dc.DiscountPercentage,
                RequiredPoints = dc.RequiredPoints,
                ExpirationDate = dc.ExpiryDate,
                UsageLimit = dc.UsageLimit,
                UsageCount = dc.UsageCount,
                Status = status,
                CreatedAt = dc.CreatedAt
            };
        }).ToList();

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.StatusFilter))
        {
            dtos = dtos.Where(d => d.Status.Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return dtos.OrderByDescending(d => d.CreatedAt).ToList();
    }
}

/// <summary>
/// Handler for getting redemption logs
/// T138: US5 - Reward System Management
/// </summary>
public class GetRedemptionsQueryHandler : IRequestHandler<GetRedemptionsQuery, List<RedemptionLogDto>>
{
    private readonly IUserDiscountRedemptionRepository _redemptionRepository;
    private readonly IUserRepository _userRepository;

    public GetRedemptionsQueryHandler(
        IUserDiscountRedemptionRepository redemptionRepository,
        IUserRepository userRepository)
    {
        _redemptionRepository = redemptionRepository;
        _userRepository = userRepository;
    }

    public async Task<List<RedemptionLogDto>> Handle(GetRedemptionsQuery request, CancellationToken cancellationToken)
    {
        var allRedemptions = await _redemptionRepository.GetAllAsync();

        // Filter by code if specified
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            allRedemptions = allRedemptions.Where(r => r.DiscountCode != null && r.DiscountCode.Code == request.Code).ToList();
        }

        // Apply date filters
        if (request.DateFrom.HasValue)
        {
            allRedemptions = allRedemptions.Where(r => r.RedeemedAt >= request.DateFrom.Value).ToList();
        }

        if (request.DateTo.HasValue)
        {
            allRedemptions = allRedemptions.Where(r => r.RedeemedAt <= request.DateTo.Value).ToList();
        }

        var userIds = allRedemptions.Select(r => r.UserId).Distinct().ToList();
        var users = await _userRepository.GetAllAsync();
        var userDict = users.Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");

        var dtos = allRedemptions.Select(r => new RedemptionLogDto
        {
            UserId = r.UserId,
            Username = userDict.GetValueOrDefault(r.UserId, "Unknown User"),
            Code = r.DiscountCode?.Code ?? "Unknown",
            RedeemedAt = r.RedeemedAt,
            DiscountAmount = r.DiscountCode?.DiscountPercentage ?? 0
        }).OrderByDescending(r => r.RedeemedAt).ToList();

        return dtos;
    }
}
