using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WahadiniCryptoQuest.Core.DTOs.Subscription;
using WahadiniCryptoQuest.Service.Commands.Admin;
using WahadiniCryptoQuest.Service.Queries.Admin;

namespace WahadiniCryptoQuest.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for managing currency pricing configurations (US5)
/// Requires Admin role authorization
/// </summary>
[ApiController]
[Route("api/admin/currency-pricing")]
[Authorize(Roles = "Admin")]
public class AdminCurrencyPricingController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCurrencyPricingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all currency pricing configurations (including inactive)
    /// </summary>
    /// <returns>List of all currency pricings</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CurrencyPricingDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllCurrencyPricingsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a single currency pricing by ID
    /// </summary>
    /// <param name="id">Currency pricing ID</param>
    /// <returns>Currency pricing details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CurrencyPricingDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCurrencyPricingQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = "Currency pricing not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new currency pricing configuration
    /// </summary>
    /// <param name="dto">Currency pricing creation data</param>
    /// <returns>Created currency pricing</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CurrencyPricingDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyPricingDto dto)
    {
        var command = new CreateCurrencyPricingCommand
        {
            CurrencyCode = dto.CurrencyCode,
            MonthlyPrice = dto.MonthlyPrice,
            YearlyPrice = dto.YearlyPrice,
            StripePriceIdMonthly = dto.StripePriceIdMonthly,
            StripePriceIdYearly = dto.StripePriceIdYearly,
            IsActive = dto.IsActive
        };

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing currency pricing configuration
    /// </summary>
    /// <param name="id">Currency pricing ID</param>
    /// <param name="dto">Updated currency pricing data</param>
    /// <returns>Updated currency pricing</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CurrencyPricingDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCurrencyPricingDto dto)
    {
        var command = new UpdateCurrencyPricingCommand
        {
            Id = id,
            CurrencyCode = dto.CurrencyCode,
            MonthlyPrice = dto.MonthlyPrice,
            YearlyPrice = dto.YearlyPrice,
            StripePriceIdMonthly = dto.StripePriceIdMonthly,
            StripePriceIdYearly = dto.StripePriceIdYearly,
            IsActive = dto.IsActive
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a currency pricing configuration
    /// </summary>
    /// <param name="id">Currency pricing ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteCurrencyPricingCommand { Id = id };

        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>
/// DTO for creating currency pricing (simplified from command)
/// </summary>
public class CreateCurrencyPricingDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string? StripePriceIdMonthly { get; set; }
    public string? StripePriceIdYearly { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating currency pricing (simplified from command)
/// </summary>
public class UpdateCurrencyPricingDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string? StripePriceIdMonthly { get; set; }
    public string? StripePriceIdYearly { get; set; }
    public bool IsActive { get; set; }
}
