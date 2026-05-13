using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;

namespace WahadiniCryptoQuest.Service.Commands.Subscription;

/// <summary>
/// Command to create Stripe checkout session for subscription purchase
/// Implements CQRS pattern with MediatR
/// </summary>
public record CreateCheckoutSessionCommand : IRequest<CheckoutSessionResponseDto>
{
    public Guid UserId { get; init; }
    public string Tier { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = "USD";
    public string? DiscountCode { get; init; }
}
