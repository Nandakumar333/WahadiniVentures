using FluentValidation;
using WahadiniCryptoQuest.Service.Discount.Queries;

namespace WahadiniCryptoQuest.API.Validators.Discount;

/// <summary>
/// Validator for GetAvailableDiscountsQuery
/// </summary>
public class GetAvailableDiscountsRequestValidator : AbstractValidator<GetAvailableDiscountsQuery>
{
    public GetAvailableDiscountsRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
