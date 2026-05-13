using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Subscription;

namespace WahadiniCryptoQuest.Service.Commands.Admin;

/// <summary>
/// Command to update an existing currency pricing configuration
/// Admin-only operation (US5)
/// </summary>
public class UpdateCurrencyPricingCommand : IRequest<CurrencyPricingDto>
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string? StripePriceIdMonthly { get; set; }
    public string? StripePriceIdYearly { get; set; }
    public bool IsActive { get; set; }
}
