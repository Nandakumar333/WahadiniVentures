using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Auth;

namespace WahadiniCryptoQuest.Service.Commands.Auth;

/// <summary>
/// Command for logging out a user and revoking their refresh token
/// Implements CQRS pattern for logout operations
/// </summary>
public class LogoutCommand : IRequest<LogoutResponse>
{
    /// <summary>
    /// The refresh token to revoke
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user performing the logout
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Creates a new logout command
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke</param>
    /// <param name="userId">The user ID performing the logout</param>
    public LogoutCommand(string refreshToken, Guid userId)
    {
        RefreshToken = refreshToken;
        UserId = userId;
    }

    /// <summary>
    /// Parameterless constructor for model binding
    /// </summary>
    public LogoutCommand() { }
}
