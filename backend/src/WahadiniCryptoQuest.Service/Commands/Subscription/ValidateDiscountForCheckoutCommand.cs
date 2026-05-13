using MediatR;

namespace WahadiniCryptoQuest.Service.Commands.Subscription;

/// <summary>
/// Command to validate and reserve a discount code for checkout
/// </summary>
public class ValidateDiscountForCheckoutCommand : IRequest<DiscountValidationResultDto>
{
    public Guid UserId { get; set; }
    public string DiscountCode { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
}

/// <summary>
/// Result of discount validation
/// </summary>
public class DiscountValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? StripeCouponId { get; set; }
}
