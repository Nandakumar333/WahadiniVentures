namespace WahadiniCryptoQuest.Core.DTOs.Auth;

/// <summary>
/// Response model for logout operation
/// </summary>
public class LogoutResponse
{
    /// <summary>
    /// Indicates if the logout was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the logout result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Creates a successful logout response
    /// </summary>
    public static LogoutResponse CreateSuccess(string message = "Logout successful")
    {
        return new LogoutResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failed logout response
    /// </summary>
    public static LogoutResponse CreateFailure(string message)
    {
        return new LogoutResponse
        {
            Success = false,
            Message = message
        };
    }
}
