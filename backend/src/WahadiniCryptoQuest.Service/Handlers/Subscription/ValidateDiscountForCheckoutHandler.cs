using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Subscription;

namespace WahadiniCryptoQuest.Service.Handlers.Subscription;

/// <summary>
/// Handler for validating discount codes for subscription checkout
/// Implements 24-hour reservation mechanism
/// </summary>
public class ValidateDiscountForCheckoutHandler : IRequestHandler<ValidateDiscountForCheckoutCommand, DiscountValidationResultDto>
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IUserDiscountRedemptionRepository _redemptionRepository;
    private readonly ILogger<ValidateDiscountForCheckoutHandler> _logger;

    public ValidateDiscountForCheckoutHandler(
        IDiscountCodeRepository discountCodeRepository,
        IUserDiscountRedemptionRepository redemptionRepository,
        ILogger<ValidateDiscountForCheckoutHandler> logger)
    {
        _discountCodeRepository = discountCodeRepository;
        _redemptionRepository = redemptionRepository;
        _logger = logger;
    }

    public async Task<DiscountValidationResultDto> Handle(ValidateDiscountForCheckoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.DiscountCode))
            {
                return new DiscountValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Discount code is required"
                };
            }

            // Parse tier
            if (!Enum.TryParse<SubscriptionTier>(request.Tier, out var tier))
            {
                return new DiscountValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Invalid subscription tier"
                };
            }

            // Find discount code
            var discountCode = await _discountCodeRepository.GetByCodeAsync(request.DiscountCode, cancellationToken);
            if (discountCode == null)
            {
                _logger.LogWarning("Discount code {Code} not found", request.DiscountCode);
                return new DiscountValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Invalid discount code"
                };
            }

            // Check if active
            if (!discountCode.IsActive)
            {
                return new DiscountValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "This discount code is no longer active"
                };
            }

            // Check expiration
            if (discountCode.ExpiryDate.HasValue && discountCode.ExpiryDate.Value < DateTime.UtcNow)
            {
                return new DiscountValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "This discount code has expired"
                };
            }

            // Check max redemptions
            if (discountCode.MaxRedemptions > 0 && discountCode.CurrentRedemptions >= discountCode.MaxRedemptions)
            {
                return new DiscountValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "This discount code has reached its usage limit"
                };
            }

            // Check if user already redeemed this code
            var hasRedeemed = await _redemptionRepository.HasUserRedeemedDiscountAsync(request.UserId, discountCode.Id, cancellationToken);
            if (hasRedeemed)
            {
                return new DiscountValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "You have already used this discount code"
                };
            }

            // Calculate discount percentage
            decimal discountPercentage = discountCode.DiscountPercentage;

            _logger.LogInformation("Validated discount code {Code} for user {UserId}: {Percentage}% off",
                request.DiscountCode, request.UserId, discountPercentage);

            return new DiscountValidationResultDto
            {
                IsValid = true,
                DiscountAmount = discountPercentage,
                StripeCouponId = null // Will be created in Stripe during checkout
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating discount code {Code} for user {UserId}",
                request.DiscountCode, request.UserId);

            return new DiscountValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "An error occurred while validating the discount code. Please try again."
            };
        }
    }
}
