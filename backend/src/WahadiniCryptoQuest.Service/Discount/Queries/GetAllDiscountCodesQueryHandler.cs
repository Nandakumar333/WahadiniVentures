using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Discount;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Discount.Queries;

/// <summary>
/// Handler for getting all discount codes (admin only)
/// </summary>
public class GetAllDiscountCodesQueryHandler : IRequestHandler<GetAllDiscountCodesQuery, List<AdminDiscountTypeDto>>
{
    private readonly IDiscountCodeRepository _discountRepository;
    private readonly ILogger<GetAllDiscountCodesQueryHandler> _logger;

    public GetAllDiscountCodesQueryHandler(
        IDiscountCodeRepository discountRepository,
        ILogger<GetAllDiscountCodesQueryHandler> logger)
    {
        _discountRepository = discountRepository;
        _logger = logger;
    }

    public async Task<List<AdminDiscountTypeDto>> Handle(GetAllDiscountCodesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all discount codes for admin");

        var discounts = await _discountRepository.GetAllAsync();

        var result = discounts
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new AdminDiscountTypeDto
            {
                Id = d.Id,
                Code = d.Code,
                DiscountPercentage = d.DiscountPercentage,
                RequiredPoints = d.RequiredPoints,
                MaxRedemptions = d.MaxRedemptions,
                CurrentRedemptions = d.CurrentRedemptions,
                ExpiryDate = d.ExpiryDate,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToList();

        _logger.LogInformation("Retrieved {Count} discount codes", result.Count);

        return result;
    }
}
