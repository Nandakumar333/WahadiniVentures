using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using WahadiniCryptoQuest.Service.Commands.Auth;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.DTOs;
using WahadiniCryptoQuest.Core.Enums;
using FluentValidation;
using System.Security.Claims;

namespace WahadiniCryptoQuest.API.Controllers;

/// <summary>
/// Authentication controller handling user registration, login, and email verification
/// Implements secure authentication workflows with proper validation and error handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<EmailConfirmationDto> _emailConfirmationValidator;
    private readonly IValidator<ResendEmailConfirmationDto> _resendEmailConfirmationValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IValidator<RegisterDto> registerValidator,
        IValidator<EmailConfirmationDto> emailConfirmationValidator,
        IValidator<ResendEmailConfirmationDto> resendEmailConfirmationValidator,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _registerValidator = registerValidator;
        _emailConfirmationValidator = emailConfirmationValidator;
        _resendEmailConfirmationValidator = resendEmailConfirmationValidator;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account with email verification
    /// </summary>
    /// <param name="registerDto">User registration data</param>
    /// <returns>Registration response with user details and verification instructions</returns>
    /// <response code="201">User successfully registered</response>
    /// <response code="400">Invalid registration data or user already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", registerDto.Email);

            // Validate the registration data
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Registration validation failed for email: {Email}", registerDto.Email);
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            // Execute registration command
            var command = new RegisterUserCommand(registerDto);
            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("User successfully registered for email: {Email}", registerDto.Email);
                return Created("", new { 
                    message = "Registration successful! Please check your email to verify your account.",
                    email = registerDto.Email 
                });
            }
            else
            {
                return BadRequest(new { error = "Registration failed" });
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed for email: {Email}", registerDto.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for email: {Email}", registerDto.Email);
            return StatusCode(500, new { error = "An unexpected error occurred during registration" });
        }
    }

    /// <summary>
    /// Confirms user email address using verification token (GET method for email links)
    /// </summary>
    /// <param name="userId">User ID from verification link</param>
    /// <param name="token">Verification token from email</param>
    /// <returns>Email confirmation result</returns>
    /// <response code="200">Email successfully confirmed</response>
    /// <response code="400">Invalid confirmation data or expired token</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmEmailGet([FromQuery] string userId, [FromQuery] string token)
    {
        // Parse userId
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            _logger.LogWarning("Invalid userId format in email confirmation: {UserId}", userId);
            return BadRequest(new { error = "Invalid user ID format" });
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Empty token in email confirmation for user: {UserId}", userId);
            return BadRequest(new { error = "Verification token is required" });
        }

        var confirmationDto = new EmailConfirmationDto
        {
            UserId = userGuid,
            Token = token
        };

        return await ConfirmEmailInternal(confirmationDto);
    }

    /// <summary>
    /// Confirms user email address using verification token (POST method for programmatic access)
    /// </summary>
    /// <param name="confirmationDto">Email confirmation data with user ID and token</param>
    /// <returns>Email confirmation result</returns>
    /// <response code="200">Email successfully confirmed</response>
    /// <response code="400">Invalid confirmation data or expired token</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmEmailPost([FromBody] EmailConfirmationDto confirmationDto)
    {
        return await ConfirmEmailInternal(confirmationDto);
    }

    /// <summary>
    /// Internal method to handle email confirmation logic
    /// </summary>
    private async Task<IActionResult> ConfirmEmailInternal(EmailConfirmationDto confirmationDto)
    {
        try
        {
            _logger.LogInformation("Email confirmation attempt for user: {UserId}", confirmationDto.UserId);

            // Validate the confirmation data
            var validationResult = await _emailConfirmationValidator.ValidateAsync(confirmationDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Email confirmation validation failed for user: {UserId}", confirmationDto.UserId);
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            // Check if user exists first to return proper 404
            var userRepo = HttpContext.RequestServices.GetRequiredService<Core.Interfaces.Repositories.IUserRepository>();
            var user = await userRepo.GetByIdAsync(confirmationDto.UserId);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation attempted for non-existent user: {UserId}", confirmationDto.UserId);
                return NotFound(new { error = "User not found" });
            }

            // Execute email confirmation command
            var command = new ConfirmEmailCommand(confirmationDto);
            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("Email successfully confirmed for user: {UserId}", confirmationDto.UserId);
                return Ok(new { 
                    success = true,
                    message = "Your email has been successfully verified! You can now login to your account." 
                });
            }
            else
            {
                _logger.LogWarning("Email confirmation failed for user: {UserId}", confirmationDto.UserId);
                return BadRequest(new { 
                    success = false,
                    message = "Invalid or expired verification link. Please request a new verification email." 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during email confirmation for user: {UserId}", confirmationDto.UserId);
            return StatusCode(500, new { error = "An unexpected error occurred during email confirmation" });
        }
    }

    /// <summary>
    /// Resends email verification to user
    /// </summary>
    /// <param name="resendDto">Email address to resend verification to</param>
    /// <returns>Success indicator</returns>
    /// <response code="200">Verification email sent successfully</response>
    /// <response code="400">Invalid email address</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("resend-email-confirmation")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationDto resendDto)
    {
        try
        {
            _logger.LogInformation("Resend email confirmation request for: {Email}", resendDto.Email);

            // Validate the resend data
            var validationResult = await _resendEmailConfirmationValidator.ValidateAsync(resendDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Resend email confirmation validation failed for: {Email}", resendDto.Email);
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            // Execute resend command
            var command = new ResendEmailConfirmationCommand(resendDto);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Email confirmation resent for: {Email}", resendDto.Email);

            return Ok(new { 
                message = "If an account with this email exists and is not yet verified, a confirmation email has been sent.",
                success = result 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during resend email confirmation for: {Email}", resendDto.Email);
            return StatusCode(500, new { error = "An unexpected error occurred while sending the verification email" });
        }
    }

    /// <summary>
    /// Gets registration status for a user (for redirect after email confirmation)
    /// </summary>
    /// <param name="userId">User ID to check status for</param>
    /// <returns>User registration status</returns>
    /// <response code="200">Registration status retrieved</response>
    /// <response code="404">User not found</response>
    [HttpGet("registration-status/{userId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetRegistrationStatus(Guid userId)
    {
        try
        {
            _logger.LogInformation("Registration status check for user: {UserId}", userId);

            // This would typically fetch user status from database
            // For now, return a placeholder response
            return Ok(new { 
                userId = userId,
                message = "Registration status endpoint - implementation pending",
                canLogin = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking registration status for user: {UserId}", userId);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Health check endpoint for authentication service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            service = "authentication"
        });
    }

    /// <summary>
    /// Protected status endpoint for testing JWT authentication
    /// </summary>
    /// <returns>User authentication status</returns>
    [HttpGet("status")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Status()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        return Ok(new { 
            status = "authenticated", 
            userId = userId,
            email = email,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Authenticates user credentials and returns JWT tokens
    /// </summary>
    /// <param name="loginRequest">User login credentials</param>
    /// <returns>Login response with JWT tokens and user information</returns>
    /// <response code="200">Login successful with tokens</response>
    /// <response code="400">Invalid login data</response>
    /// <response code="401">Invalid credentials or unconfirmed email</response>
    /// <response code="423">Account locked due to failed attempts</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status423Locked)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginRequest.Email);

            // Extract device info and IP address from request context
            var deviceInfo = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Create login command - use the existing command structure
            var command = new LoginUserCommand
            {
                Email = loginRequest.Email,
                Password = loginRequest.Password,
                RememberMe = loginRequest.RememberMe,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress
            };

            // Execute login command
            var result = await _mediator.Send(command);

            if (result != null && !string.IsNullOrEmpty(result.AccessToken))
            {
                _logger.LogInformation("Login successful for email: {Email}", loginRequest.Email);
                
                // Convert AuthResponseDto to LoginResponse
                var loginResponse = new LoginResponse
                {
                    Success = true,
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresIn = (int)(result.ExpiresAt - DateTime.UtcNow).TotalSeconds,
                    User = result.User
                };
                
                return Ok(loginResponse);
            }
            else
            {
                _logger.LogWarning("Login failed for email: {Email}", loginRequest.Email);
                return Unauthorized(new { error = "Invalid email or password" });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for email: {Email}. Reason: {Reason}", 
                loginRequest.Email, ex.Message);

            // Check if it's an account lockout
            if (ex.Message.Contains("locked"))
            {
                return StatusCode(423, new { error = ex.Message });
            }

            // Check if it's an email confirmation issue
            if (ex.Message.Contains("confirm your email"))
            {
                return Unauthorized(new { error = ex.Message, requiresEmailConfirmation = true });
            }

            // Default to 401 for invalid credentials
            return Unauthorized(new { error = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Login validation failed for email: {Email}. Errors: {Errors}", 
                loginRequest.Email, string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
            
            // Convert validation errors to model state
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", loginRequest.Email);
            return StatusCode(500, new { error = "An unexpected error occurred during login" });
        }
    }

    /// <summary>
    /// Refreshes an access token using a valid refresh token
    /// </summary>
    /// <param name="request">Refresh token request containing the refresh token</param>
    /// <returns>New access and refresh tokens, or error if invalid</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Invalid refresh token request: {Errors}", string.Join(", ", errors));
                return BadRequest(new { error = "Invalid request", details = errors });
            }

            // Get device info and IP address (prioritize request body, fallback to HTTP context)
            var deviceInfo = !string.IsNullOrEmpty(request.DeviceInfo) 
                ? request.DeviceInfo 
                : HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            var ipAddress = !string.IsNullOrEmpty(request.IpAddress) 
                ? request.IpAddress 
                : HttpContext.Connection.RemoteIpAddress?.ToString();

            var command = new RefreshTokenCommand(request.RefreshToken, deviceInfo, ipAddress);
            var result = await _mediator.Send(command);

            if (result?.Success == true)
            {
                _logger.LogInformation("Token refresh successful");
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Token refresh failed: {Error}", result?.ErrorMessage ?? "Unknown error");
                return Unauthorized(new { error = result?.ErrorMessage ?? "Token refresh failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return StatusCode(500, new { error = "An unexpected error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token
    /// </summary>
    /// <param name="request">Logout request containing the refresh token to revoke</param>
    /// <returns>Success message after revoking the token</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Invalid logout request: {Errors}", string.Join(", ", errors));
                return BadRequest(new { error = "Invalid request", details = errors });
            }

            // Get the user ID from the authenticated user's claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Logout request with invalid user ID claim");
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            var command = new LogoutCommand(request.RefreshToken, userId);
            var result = await _mediator.Send(command);

            if (result?.Success == true)
            {
                _logger.LogInformation("Logout successful for user {UserId}", userId);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Logout failed for user {UserId}: {Error}", userId, result?.Message ?? "Unknown error");
                return BadRequest(new { error = result?.Message ?? "Logout failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout");
            return StatusCode(500, new { error = "An unexpected error occurred during logout" });
        }
    }

    /// <summary>
    /// Initiates password reset process by sending reset email
    /// </summary>
    /// <param name="request">Password reset request containing user email</param>
    /// <returns>Success message regardless of whether email exists (security)</returns>
    [HttpPost("password-reset/request")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Invalid password reset request: {Errors}", string.Join(", ", errors));
                return BadRequest(new { error = "Invalid request", details = errors });
            }

            // Get client info and IP address from request headers
            var clientInfo = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var command = new PasswordResetRequestCommand(request.Email, clientInfo, ipAddress);
            var result = await _mediator.Send(command);

            // Always return 200 OK for security (prevent email enumeration)
            _logger.LogInformation("Password reset request processed for email: {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password reset request");
            return StatusCode(500, new { error = "An unexpected error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Confirms password reset with new password using reset token
    /// </summary>
    /// <param name="request">Password reset confirmation with token and new password</param>
    /// <returns>Success or failure response</returns>
    [HttpPost("password-reset/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] PasswordResetConfirmRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Invalid password reset confirmation: {Errors}", string.Join(", ", errors));
                return BadRequest(new { error = "Invalid request", details = errors });
            }

            // Get client info and IP address from request headers
            var clientInfo = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var command = new PasswordResetConfirmCommand(request.Token, request.NewPassword, clientInfo, ipAddress);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _logger.LogInformation("Password reset confirmation successful");
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Password reset confirmation failed: {Error}", result.ErrorMessage);
                return Unauthorized(new { error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password reset confirmation");
            return StatusCode(500, new { error = "An unexpected error occurred while resetting your password" });
        }
    }
}