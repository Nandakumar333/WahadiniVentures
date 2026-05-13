using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Auth;

namespace WahadiniCryptoQuest.Service.Commands.Auth;

/// <summary>
/// Command for initiating password reset process
/// Generates reset token and sends email to user
/// </summary>
public class PasswordResetRequestCommand : IRequest<PasswordResetResponse>
{
    /// <summary>
    /// Email address of the user requesting password reset
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Client information for tracking and security purposes
    /// </summary>
    public string? ClientInfo { get; set; }

    /// <summary>
    /// IP address of the client making the request
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Creates a new password reset request command
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="clientInfo">Client information</param>
    /// <param name="ipAddress">Client IP address</param>
    public PasswordResetRequestCommand(string email, string? clientInfo = null, string? ipAddress = null)
    {
        Email = email;
        ClientInfo = clientInfo;
        IpAddress = ipAddress;
    }
}