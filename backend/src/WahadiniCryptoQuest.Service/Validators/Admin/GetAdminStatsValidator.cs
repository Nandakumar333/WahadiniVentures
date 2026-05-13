using FluentValidation;
using WahadiniCryptoQuest.Service.Queries.Admin;

namespace WahadiniCryptoQuest.Service.Validators.Admin;

/// <summary>
/// Validator for GetAdminStatsQuery
/// Placeholder for consistency - no validation needed as query has no parameters (US1)
/// </summary>
public class GetAdminStatsValidator : AbstractValidator<GetAdminStatsQuery>
{
    public GetAdminStatsValidator()
    {
        // No validation rules - query has no input parameters
        // Placeholder maintained for architectural consistency
    }
}
