using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Service.Commands.Auth;

namespace WahadiniCryptoQuest.Service.Handlers.Auth;

/// <summary>
/// Handler for password reset confirmation command
/// Validates reset token and updates user password
/// </summary>
public class PasswordResetConfirmCommandHandler : IRequestHandler<PasswordResetConfirmCommand, PasswordResetResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashingService _passwordHashService;
    private readonly ILogger<PasswordResetConfirmCommandHandler> _logger;

    public PasswordResetConfirmCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHashingService passwordHashService,
        ILogger<PasswordResetConfirmCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHashService = passwordHashService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the password reset confirmation command
    /// Validates token and updates user password
    /// </summary>
    /// <param name="request">Password reset confirmation command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Password reset response</returns>
    public async Task<PasswordResetResponse> Handle(PasswordResetConfirmCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing password reset confirmation");

            // Validate input
            if (string.IsNullOrEmpty(request.Token))
            {
                _logger.LogWarning("Password reset confirmation received with empty token");
                return PasswordResetResponse.CreateFailure("Reset token is required");
            }

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                _logger.LogWarning("Password reset confirmation received with empty password");
                return PasswordResetResponse.CreateFailure("New password is required");
            }

            // Find the reset token
            var resetToken = await _unitOfWork.PasswordResetTokens.GetValidTokenAsync(request.Token, cancellationToken);
            if (resetToken == null)
            {
                _logger.LogWarning("Invalid or expired password reset token provided");
                return PasswordResetResponse.CreateFailure("Invalid or expired reset token");
            }

            // Validate the token matches the provided raw token
            if (!resetToken.MatchesToken(request.Token))
            {
                _logger.LogWarning("Password reset token does not match stored hash");
                return PasswordResetResponse.CreateFailure("Invalid reset token");
            }

            // Check if token is still valid
            if (!resetToken.IsValid)
            {
                _logger.LogWarning("Password reset token is no longer valid - UserId: {UserId}, IsExpired: {IsExpired}, IsUsed: {IsUsed}", 
                    resetToken.UserId, resetToken.IsExpired, resetToken.IsUsed);
                
                return PasswordResetResponse.CreateFailure("Reset token has expired or has already been used");
            }

            // Get the user
            var user = await _unitOfWork.Users.GetByIdAsync(resetToken.UserId);
            if (user == null)
            {
                _logger.LogError("User not found for password reset token - UserId: {UserId}", resetToken.UserId);
                return PasswordResetResponse.CreateFailure("User account not found");
            }

            // Check if user account is still active
            if (!user.IsActive || user.IsDeleted)
            {
                _logger.LogWarning("Inactive user attempted password reset - UserId: {UserId}", user.Id);
                return PasswordResetResponse.CreateFailure("Account is inactive");
            }

            // Hash the new password
            var hashedPassword = _passwordHashService.HashPassword(request.NewPassword);

            // Update user password
            user.UpdatePassword(hashedPassword);

            // Mark the reset token as used
            resetToken.MarkAsUsed("Password Reset Confirmation");

            // Revoke all existing refresh tokens for security (force re-login)
            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, "Password Reset", cancellationToken);

            // Save changes
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.PasswordResetTokens.UpdateAsync(resetToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Clean up any other password reset tokens for this user
            await _unitOfWork.PasswordResetTokens.InvalidateAllUserTokensAsync(user.Id, "Password Reset Cleanup", cancellationToken);

            _logger.LogInformation("Password reset completed successfully for user {UserId}", user.Id);

            return PasswordResetResponse.CreateConfirmSuccess(
                "Your password has been reset successfully. You can now log in with your new password."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset confirmation");
            return PasswordResetResponse.CreateFailure("An error occurred while resetting your password. Please try again later.");
        }
    }
}