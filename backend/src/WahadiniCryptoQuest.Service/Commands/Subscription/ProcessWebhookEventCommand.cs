using MediatR;

namespace WahadiniCryptoQuest.Service.Commands.Subscription;

/// <summary>
/// Command to process a Stripe webhook event
/// </summary>
public class ProcessWebhookEventCommand : IRequest<bool>
{
    public string StripeEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime EventCreatedAt { get; set; }
}
