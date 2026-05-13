using FluentValidation;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Validators.Admin;

/// <summary>
/// Validator for CreateCurrencyPricingCommand
/// Enforces 0-9999 range and 50% deviation warnings (US5)
/// </summary>
public class CreateCurrencyPricingValidator : AbstractValidator<CreateCurrencyPricingCommand>
{
    private const decimal MinPrice = 0;
    private const decimal MaxPrice = 9999;
    private const decimal MaxDeviationPercent = 0.50m; // 50%

    public CreateCurrencyPricingValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3).WithMessage("Currency code must be 3 characters (ISO 4217)")
            .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase letters");

        RuleFor(x => x.MonthlyPrice)
            .InclusiveBetween(MinPrice, MaxPrice)
            .WithMessage($"Monthly price must be between {MinPrice} and {MaxPrice}");

        RuleFor(x => x.YearlyPrice)
            .InclusiveBetween(MinPrice, MaxPrice)
            .WithMessage($"Yearly price must be between {MinPrice} and {MaxPrice}");

        // Yearly should generally be less than 12x monthly (savings expected)
        RuleFor(x => x)
            .Must(x => x.YearlyPrice <= x.MonthlyPrice * 12)
            .WithMessage("Yearly price should be less than or equal to 12 times the monthly price")
            .When(x => x.MonthlyPrice > 0 && x.YearlyPrice > 0);

        // Deviation warning - yearly should be within 50% of 12x monthly
        RuleFor(x => x)
            .Must(x =>
            {
                if (x.MonthlyPrice == 0 || x.YearlyPrice == 0) return true;
                var expectedYearly = x.MonthlyPrice * 12;
                var deviation = Math.Abs(x.YearlyPrice - expectedYearly) / expectedYearly;
                return deviation <= MaxDeviationPercent;
            })
            .WithMessage($"Yearly price deviates more than {MaxDeviationPercent * 100}% from expected annual price (12x monthly)")
            .When(x => x.MonthlyPrice > 0 && x.YearlyPrice > 0);

        RuleFor(x => x.StripePriceIdMonthly)
            .MaximumLength(255).WithMessage("Stripe Price ID (Monthly) cannot exceed 255 characters");

        RuleFor(x => x.StripePriceIdYearly)
            .MaximumLength(255).WithMessage("Stripe Price ID (Yearly) cannot exceed 255 characters");
    }
}
