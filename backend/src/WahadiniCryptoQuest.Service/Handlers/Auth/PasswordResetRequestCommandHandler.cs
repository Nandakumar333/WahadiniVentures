using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Service.Commands.Auth;

namespace WahadiniCryptoQuest.Service.Handlers.Auth;

/// <summary>
/// Handler for password reset request command
/// Generates password reset token and sends email to user
/// </summary>
public class PasswordResetRequestCommandHandler : IRequestHandler<PasswordResetRequestCommand, PasswordResetResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<PasswordResetRequestCommandHandler> _logger;

    public PasswordResetRequestCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<PasswordResetRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the password reset request command
    /// Creates reset token and sends email to user
    /// </summary>
    /// <param name="request">Password reset request command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Password reset response</returns>
    public async Task<PasswordResetResponse> Handle(PasswordResetRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing password reset request for email: {Email}", request.Email);

            // Validate input
            if (string.IsNullOrEmpty(request.Email))
            {
                _logger.LogWarning("Password reset request received with empty email");
                return PasswordResetResponse.CreateFailure("Email address is required");
            }

            // Find the user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            
            // For security reasons, always return success even if user doesn't exist
            // This prevents email enumeration attacks
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                
                // Return success response to prevent email enumeration
                return PasswordResetResponse.CreateRequestSuccess(
                    "If an account with that email exists, a password reset link has been sent.",
                    emailSent: false
                );
            }

            // Check if user account is active
            if (!user.IsActive || user.IsDeleted)
            {
                _logger.LogWarning("Password reset requested for inactive user: {UserId}", user.Id);
                
                // Return success response to prevent information disclosure
                return PasswordResetResponse.CreateRequestSuccess(
                    "If an account with that email exists, a password reset link has been sent.",
                    emailSent: false
                );
            }

            // Invalidate any existing password reset tokens for this user
            await _unitOfWork.PasswordResetTokens.InvalidateAllUserTokensAsync(user.Id, "Password Reset Request", cancellationToken);

            // Create new password reset token (1 hour expiration)
            var resetToken = PasswordResetToken.Create(
                user.Id,
                request.ClientInfo,
                request.IpAddress
            );

            // Save the reset token
            await _unitOfWork.PasswordResetTokens.AddAsync(resetToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send password reset email
            var emailSent = false;
            try
            {
                await _emailService.SendPasswordResetEmailAsync(
                    user.Email,
                    user.FullName,
                    resetToken.Token,
                    resetToken.ExpiresAt
                );
                emailSent = true;
                _logger.LogInformation("Password reset email sent successfully to user {UserId}", user.Id);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send password reset email to user {UserId}", user.Id);
                
                // Mark token as used since email failed
                resetToken.MarkAsUsed("Email Send Failed");
                await _unitOfWork.PasswordResetTokens.UpdateAsync(resetToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                return PasswordResetResponse.CreateFailure(
                    "An error occurred while sending the password reset email. Please try again later."
                );
            }

            _logger.LogInformation("Password reset token created successfully for user {UserId}", user.Id);

            return PasswordResetResponse.CreateRequestSuccess(
                "If an account with that email exists, a password reset link has been sent.",
                emailSent: emailSent,
                tokenExpiresAt: resetToken.ExpiresAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset request for email: {Email}", request.Email);
            return PasswordResetResponse.CreateFailure("An error occurred while processing your request. Please try again later.");
        }
    }
}