using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.Service.Commands.Auth;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using AutoMapper;
using BCrypt.Net;

namespace WahadiniCryptoQuest.Service.Handlers.Auth;

/// <summary>
/// Command handler for user registration
/// Implements the registration workflow with email verification
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IMapper mapper,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting user registration for email: {Email}", request.RegisterData.Email);

            // Check if user already exists
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.RegisterData.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt for existing email: {Email}", request.RegisterData.Email);
                throw new InvalidOperationException("An account with this email address already exists");
            }

            // Hash the password using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.RegisterData.Password);

            // Create new user entity using factory method
            var user = User.Create(
                email: request.RegisterData.Email,
                passwordHash: passwordHash,
                firstName: request.RegisterData.FirstName,
                lastName: request.RegisterData.LastName
            );

            // Save user to database
            var savedUser = await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User created with ID: {UserId}", savedUser.Id);

            // Create email verification token
            var verificationToken = EmailVerificationToken.Create(savedUser.Id, 24); // 24 hours expiration
            await _unitOfWork.EmailVerificationTokens.AddAsync(verificationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Verification token created for user: {UserId}", savedUser.Id);

            // Send verification email
            await _emailService.SendEmailVerificationAsync(
                savedUser.Email,
                savedUser.FirstName,
                savedUser.Id,
                verificationToken.Token
            );

            _logger.LogInformation("Verification email sent to: {Email}", savedUser.Email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.RegisterData.Email);
            throw;
        }
    }
}

/// <summary>
/// Command handler for email confirmation
/// Implements the email verification workflow
/// </summary>
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ConfirmEmailCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing email confirmation for user: {UserId}", request.ConfirmationData.UserId);

            // Find the user
            var user = await _unitOfWork.Users.GetByIdAsync(request.ConfirmationData.UserId);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation attempted for non-existent user: {UserId}", request.ConfirmationData.UserId);
                return false;
            }

            // First, validate the token regardless of user status (security check)
            var token = await _unitOfWork.EmailVerificationTokens.GetByTokenAsync(request.ConfirmationData.Token);
            if (token == null || token.UserId != user.Id)
            {
                _logger.LogWarning("Invalid verification token used for user: {UserId}", request.ConfirmationData.UserId);
                return false;
            }

            // Check if token is already used or expired (security check)
            if (token.IsUsed)
            {
                _logger.LogWarning("Already used verification token for user: {UserId}", request.ConfirmationData.UserId);
                return false;
            }

            if (token.IsExpired())
            {
                _logger.LogWarning("Expired verification token for user: {UserId}", request.ConfirmationData.UserId);
                return false;
            }

            // Check if email is already confirmed
            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email confirmation attempted for already confirmed user: {UserId}", request.ConfirmationData.UserId);
                // Still mark token as used to prevent reuse
                token.MarkAsUsed();
                await _unitOfWork.EmailVerificationTokens.UpdateAsync(token);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }

            // Confirm the user's email
            user.ConfirmEmail();
            await _unitOfWork.Users.UpdateAsync(user);

            // Mark the token as used
            token.MarkAsUsed();
            await _unitOfWork.EmailVerificationTokens.UpdateAsync(token);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email successfully confirmed for user: {UserId}", user.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email confirmation for user: {UserId}", request.ConfirmationData.UserId);
            return false;
        }
    }
}

/// <summary>
/// Command handler for resending email confirmation
/// Generates new verification token and sends email
/// </summary>
public class ResendEmailConfirmationCommandHandler : IRequestHandler<ResendEmailConfirmationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResendEmailConfirmationCommandHandler> _logger;

    public ResendEmailConfirmationCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<ResendEmailConfirmationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing resend email confirmation for: {Email}", request.ResendData.Email);

            // Find the user
            var user = await _unitOfWork.Users.GetByEmailAsync(request.ResendData.Email);
            if (user == null)
            {
                _logger.LogWarning("Resend email confirmation attempted for non-existent email: {Email}", request.ResendData.Email);
                // Return true to prevent email enumeration attacks
                return true;
            }

            // Check if email is already confirmed
            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Resend email confirmation attempted for already confirmed user: {Email}", request.ResendData.Email);
                // Return true - email is already confirmed
                return true;
            }

            // Invalidate all existing tokens for this user
            await _unitOfWork.EmailVerificationTokens.InvalidateAllUserTokensAsync(user.Id);

            // Create new verification token
            var verificationToken = EmailVerificationToken.Create(user.Id, 24); // 24 hours expiration
            await _unitOfWork.EmailVerificationTokens.AddAsync(verificationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("New verification token created for user: {UserId}", user.Id);

            // Send verification email
            await _emailService.SendEmailVerificationAsync(
                user.Email,
                user.FirstName,
                user.Id,
                verificationToken.Token
            );

            _logger.LogInformation("Verification email resent to: {Email}", user.Email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resend email confirmation for: {Email}", request.ResendData.Email);
            return false;
        }
    }
}