using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Service.Commands.Auth;

namespace WahadiniCryptoQuest.Service.Handlers.Auth;

/// <summary>
/// Handler for logout command
/// Revokes the user's refresh token to prevent further token refreshes
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<LogoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the logout command
    /// Revokes the refresh token to prevent further use
    /// </summary>
    /// <param name="request">Logout command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Logout response indicating success or failure</returns>
    public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing logout request for user {UserId}", request.UserId);

            // Validate input
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                _logger.LogWarning("Logout request received with empty refresh token");
                return LogoutResponse.CreateFailure("Refresh token is required");
            }

            // Find the refresh token
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found during logout: {Token}", request.RefreshToken[..8] + "...");
                // Return success even if token not found (already logged out or invalid token)
                return LogoutResponse.CreateSuccess("Logged out successfully");
            }

            // Verify the token belongs to the user
            if (refreshToken.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to revoke token belonging to user {TokenUserId}", 
                    request.UserId, refreshToken.UserId);
                return LogoutResponse.CreateFailure("Invalid refresh token");
            }

            // Revoke the token
            if (!refreshToken.IsRevoked)
            {
                refreshToken.Revoke($"User {request.UserId}");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Refresh token revoked for user {UserId}", request.UserId);
            }
            else
            {
                _logger.LogInformation("Refresh token already revoked for user {UserId}", request.UserId);
            }

            return LogoutResponse.CreateSuccess("Logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing logout request for user {UserId}", request.UserId);
            return LogoutResponse.CreateFailure("An error occurred during logout");
        }
    }
}
