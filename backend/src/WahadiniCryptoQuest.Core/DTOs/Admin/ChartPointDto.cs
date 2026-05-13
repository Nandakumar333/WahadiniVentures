using System;

namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// Represents a single data point for chart rendering
/// </summary>
public class ChartPointDto
{
    /// <summary>
    /// Date of the data point
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Numeric value at this date (count, revenue, etc.)
    /// </summary>
    public decimal Value { get; set; }
}
