using MediatR;

namespace WahadiniCryptoQuest.Service.Commands.Subscription;

/// <summary>
/// Command to create a Stripe billing portal session for subscription management
/// </summary>
public class CreatePortalSessionCommand : IRequest<PortalSessionResponseDto>
{
    public Guid UserId { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
}

/// <summary>
/// Response containing portal session URL
/// </summary>
public class PortalSessionResponseDto
{
    public string PortalUrl { get; set; } = string.Empty;
}
