using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Auth;

namespace WahadiniCryptoQuest.Service.Commands.Auth;

/// <summary>
/// Command for confirming password reset with new password
/// Validates reset token and updates user password
/// </summary>
public class PasswordResetConfirmCommand : IRequest<PasswordResetResponse>
{
    /// <summary>
    /// Password reset token received via email
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// New password for the user account
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Client information for tracking and security purposes
    /// </summary>
    public string? ClientInfo { get; set; }

    /// <summary>
    /// IP address of the client making the request
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Creates a new password reset confirmation command
    /// </summary>
    /// <param name="token">Reset token</param>
    /// <param name="newPassword">New password</param>
    /// <param name="clientInfo">Client information</param>
    /// <param name="ipAddress">Client IP address</param>
    public PasswordResetConfirmCommand(string token, string newPassword, string? clientInfo = null, string? ipAddress = null)
    {
        Token = token;
        NewPassword = newPassword;
        ClientInfo = clientInfo;
        IpAddress = ipAddress;
    }
}