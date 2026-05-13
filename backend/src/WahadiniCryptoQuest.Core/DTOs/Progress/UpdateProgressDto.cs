using System.ComponentModel.DataAnnotations;

namespace WahadiniCryptoQuest.Core.DTOs.Progress;

/// <summary>
/// DTO for updating lesson watch progress
/// </summary>
public class UpdateProgressDto
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Watch position must be non-negative")]
    public int WatchPosition { get; set; }
}
