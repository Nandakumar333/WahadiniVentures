using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Auth;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.DTOs;

namespace WahadiniCryptoQuest.Service.Handlers.Auth;

/// <summary>
/// Handler for user login command
/// </summary>
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHashingService passwordHashingService,
        IJwtTokenService jwtTokenService,
        IAuthorizationService authorizationService,
        IMapper mapper,
        ILogger<LoginUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHashingService = passwordHashingService;
        _jwtTokenService = jwtTokenService;
        _authorizationService = authorizationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing login attempt for email: {Email}", request.Email);

            // Get user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed - user not found for email: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt failed - inactive user: {UserId}", user.Id);
                throw new UnauthorizedAccessException("Account is inactive. Please contact support.");
            }

            // Check if account is locked
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                var remainingLockout = user.LockoutEnd.Value.Subtract(DateTime.UtcNow);
                _logger.LogWarning("Login attempt failed - account locked for user: {UserId}, remaining: {Remaining}", 
                    user.Id, remainingLockout);
                throw new UnauthorizedAccessException($"Account is locked. Please try again in {remainingLockout.Minutes} minutes.");
            }

            // Verify password
            if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login attempt failed - invalid password for user: {UserId}", user.Id);
                
                // Increment failed attempts
                user.IncrementFailedLoginAttempts();
                await _unitOfWork.Users.UpdateAsync(user);
                
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login attempt failed - email not confirmed for user: {UserId}", user.Id);
                throw new UnauthorizedAccessException("Please confirm your email address before logging in.");
            }

            // Login successful - record login and reset failed attempts
            user.RecordLogin();
            await _unitOfWork.Users.UpdateAsync(user);

            _logger.LogInformation("User role after fetch: {Role} (int value: {RoleInt})", user.Role, (int)user.Role);

            // Generate access token with user's role and permissions
            var roles = new[] { user.Role.ToString() }; // Use actual role from User entity
            
            // Load user permissions from database
            var permissions = await _authorizationService.GetUserPermissionsAsync(user.Id);
            
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(
                user.Id, user.Email, user.FullName, roles, user.EmailConfirmed, permissions);
            
            // Create and save refresh token entity
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // 7 days
            var refreshTokenEntity = RefreshToken.Create(
                user.Id,
                refreshTokenExpiry,
                request.DeviceInfo,
                request.IpAddress);
            
            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Get user's roles from UserRole table or fallback to user's base role
            var userRoles = await _authorizationService.GetUserRolesAsync(user.Id);
            var roleNames = userRoles?.ToList() ?? new List<string>();
            
            _logger.LogInformation("Retrieved {Count} roles from UserRole table: {Roles}", roleNames.Count, string.Join(", ", roleNames));
            
            // Ensure we always have at least the user's base role
            if (!roleNames.Any())
            {
                roleNames.Add(user.Role.ToString());
                _logger.LogWarning("No roles found in UserRole table, using fallback: {Role}", user.Role.ToString());
            }
            
            // Map user to DTO
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Email, // Use email as username for now
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                SubscriptionTier = (Core.Enums.SubscriptionTier)(int)user.Role,
                Role = (int)user.Role, // Add role as integer for frontend (0=Free, 1=Premium, 2=Admin)
                Roles = roleNames, // Add roles array
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            _logger.LogInformation("UserDto created with Role={Role}, Roles={Roles}", userDto.Role, string.Join(", ", userDto.Roles));

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenEntity.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutes from now
                User = userDto
            };

            _logger.LogInformation("Login successful for user: {UserId}", user.Id);
            return response;
        }
        catch (UnauthorizedAccessException)
        {
            // Re-throw authorization exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for email: {Email}", request.Email);
            throw new InvalidOperationException("An error occurred during login. Please try again.");
        }
    }
}