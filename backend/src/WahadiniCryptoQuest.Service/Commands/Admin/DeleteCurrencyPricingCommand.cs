using MediatR;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to delete a currency pricing configuration
/// Admin-only operation (US5)
/// </summary>
public class DeleteCurrencyPricingCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
