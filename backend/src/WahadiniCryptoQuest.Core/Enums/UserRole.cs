namespace WahadiniCryptoQuest.Core.Enums;

/// <summary>
/// User authorization roles (separate from subscription tiers)
/// </summary>
public enum UserRole
{
    User,            // Standard user (default role)
    Admin,           // Platform administrator
    ContentCreator,  // Can create and manage courses
    Moderator        // Can review task submissions
}
