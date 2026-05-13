namespace WahadiniCryptoQuest.Core.Enums;

/// <summary>
/// User roles for role-based access control
/// Following the freemium business model with clear tier progression
/// </summary>
public enum UserRoleEnum
{
    /// <summary>
    /// Free tier users - access to basic content and features
    /// </summary>
    Free = 0,

    /// <summary>
    /// Premium users - access to premium content and advanced features
    /// Monthly/yearly subscription required
    /// </summary>
    Premium = 1,

    /// <summary>
    /// Admin users - full system access and management capabilities
    /// </summary>
    Admin = 2,

    /// <summary>
    /// SuperAdmin users - highest privilege level with ability to manage admins
    /// Only SuperAdmin can ban/unban admins and change user roles
    /// </summary>
    SuperAdmin = 3
}