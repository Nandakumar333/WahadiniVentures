using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.DTOs;

/// <summary>
/// Data transfer object for user information
/// </summary>
public class UserDto
{
    /// <summary>
    /// User's unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's username (can be display name or actual username)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user's email has been confirmed
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// User's subscription tier
    /// </summary>
    public SubscriptionTier SubscriptionTier { get; set; }

    /// <summary>
    /// User's role (0=Free, 1=Premium, 2=Admin) - for frontend compatibility
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// User's roles as string array - for frontend compatibility
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// User's total reward points
    /// </summary>
    public int TotalRewardPoints { get; set; }

    /// <summary>
    /// When the premium subscription expires (if applicable)
    /// </summary>
    public DateTime? PremiumExpiresAt { get; set; }

    /// <summary>
    /// When the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}