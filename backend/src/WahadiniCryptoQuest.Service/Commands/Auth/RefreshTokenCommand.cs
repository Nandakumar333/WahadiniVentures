using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Auth;

namespace WahadiniCryptoQuest.Service.Commands.Auth;

/// <summary>
/// Command for refreshing an access token using a refresh token
/// Implements CQRS pattern for token refresh operations
/// </summary>
public class RefreshTokenCommand : IRequest<RefreshTokenResponse>
{
    /// <summary>
    /// The refresh token to exchange for a new access token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional device information for tracking and security
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Optional IP address for security tracking
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Creates a new refresh token command
    /// </summary>
    /// <param name="refreshToken">The refresh token string</param>
    /// <param name="deviceInfo">Optional device information</param>
    /// <param name="ipAddress">Optional IP address</param>
    public RefreshTokenCommand(string refreshToken, string? deviceInfo = null, string? ipAddress = null)
    {
        RefreshToken = refreshToken;
        DeviceInfo = deviceInfo;
        IpAddress = ipAddress;
    }

    /// <summary>
    /// Parameterless constructor for model binding
    /// </summary>
    public RefreshTokenCommand() { }
}