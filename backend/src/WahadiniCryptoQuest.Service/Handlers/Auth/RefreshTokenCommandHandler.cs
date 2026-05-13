using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Service.Commands.Auth;

namespace WahadiniCryptoQuest.Service.Handlers.Auth;

/// <summary>
/// Handler for refresh token command
/// Implements secure token refresh with rotation and validation
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IAuthorizationService authorizationService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the refresh token command
    /// Validates the refresh token and generates new tokens
    /// </summary>
    /// <param name="request">Refresh token command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New tokens or null if invalid</returns>
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing refresh token request");

            // Validate input
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                _logger.LogWarning("Refresh token request received with empty token");
                return RefreshTokenResponse.CreateFailure("Refresh token is required");
            }

            // Find the refresh token
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", request.RefreshToken[..8] + "...");
                return RefreshTokenResponse.CreateFailure("Invalid refresh token");
            }

            // Validate the refresh token
            if (!refreshToken.IsValid)
            {
                _logger.LogWarning("Invalid refresh token used - UserId: {UserId}, IsExpired: {IsExpired}, IsUsed: {IsUsed}, IsRevoked: {IsRevoked}", 
                    refreshToken.UserId, refreshToken.IsExpired, refreshToken.IsUsed, refreshToken.IsRevoked);
                
                // If token is expired, clean up expired tokens for this user
                if (refreshToken.IsExpired)
                {
                    await _unitOfWork.RefreshTokens.RemoveExpiredTokensAsync(DateTime.UtcNow.AddDays(-1), cancellationToken);
                }

                return RefreshTokenResponse.CreateFailure("Refresh token is invalid or expired");
            }

            // Get the user
            var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                _logger.LogError("User not found for refresh token - UserId: {UserId}", refreshToken.UserId);
                return RefreshTokenResponse.CreateFailure("User not found");
            }

            // Check if user account is still active
            if (!user.IsActive || user.IsDeleted)
            {
                _logger.LogWarning("Inactive user attempted token refresh - UserId: {UserId}", user.Id);
                // Revoke all tokens for inactive user
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, "System - Inactive Account", cancellationToken);
                return RefreshTokenResponse.CreateFailure("Account is inactive");
            }

            // Mark the current refresh token as revoked (token rotation for security)
            refreshToken.Revoke("Token Refresh - Rotation");

            // Update device info if provided
            if (!string.IsNullOrEmpty(request.DeviceInfo) || !string.IsNullOrEmpty(request.IpAddress))
            {
                refreshToken.UpdateDeviceInfo(request.DeviceInfo, request.IpAddress);
            }

            // Save the changes to the old token
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate new tokens with permissions
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(15); // 15 minutes
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(30); // 30 days

            // Invalidate cache to ensure fresh permissions are loaded (in case role changed)
            _authorizationService.InvalidateUserPermissionsCache(user.Id);
            
            // Load user permissions from database
            var permissions = await _authorizationService.GetUserPermissionsAsync(user.Id);

            var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync(
                user.Id, 
                user.Email, 
                user.FullName, 
                new[] { user.Role.ToString() },
                user.EmailConfirmed,
                permissions);
            var newRefreshToken = RefreshToken.Create(
                user.Id, 
                refreshTokenExpiry, 
                request.DeviceInfo, 
                request.IpAddress);

            // Save the new refresh token
            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully refreshed tokens for user {UserId}", user.Id);

            // Return the response
            return RefreshTokenResponse.CreateSuccess(
                newAccessToken, 
                newRefreshToken.Token, 
                accessTokenExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refresh token request");
            return RefreshTokenResponse.CreateFailure("An error occurred while refreshing the token");
        }
    }
}