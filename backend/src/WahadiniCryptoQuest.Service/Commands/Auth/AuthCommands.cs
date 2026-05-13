using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Auth;

namespace WahadiniCryptoQuest.Service.Commands.Auth;

/// <summary>
/// Command for user registration with email verification
/// Uses RegisterDto for input validation
/// </summary>
public class RegisterUserCommand : IRequest<bool>
{
    public RegisterDto RegisterData { get; set; } = new();

    public RegisterUserCommand(RegisterDto registerData)
    {
        RegisterData = registerData;
    }

    // Parameterless constructor for model binding
    public RegisterUserCommand() { }
}

/// <summary>
/// Command for confirming email address
/// Uses EmailConfirmationDto for input validation
/// </summary>
public class ConfirmEmailCommand : IRequest<bool>
{
    public EmailConfirmationDto ConfirmationData { get; set; } = new();

    public ConfirmEmailCommand(EmailConfirmationDto confirmationData)
    {
        ConfirmationData = confirmationData;
    }

    // Parameterless constructor for model binding
    public ConfirmEmailCommand() { }
}

/// <summary>
/// Command for resending email confirmation
/// Uses ResendEmailConfirmationDto for input validation
/// </summary>
public class ResendEmailConfirmationCommand : IRequest<bool>
{
    public ResendEmailConfirmationDto ResendData { get; set; } = new();

    public ResendEmailConfirmationCommand(ResendEmailConfirmationDto resendData)
    {
        ResendData = resendData;
    }

    // Parameterless constructor for model binding
    public ResendEmailConfirmationCommand() { }
}

/// <summary>
/// Command for user login
/// </summary>
public class LoginUserCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// Command for password reset request
/// </summary>
public class ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Command for resetting password
/// </summary>
public class ResetPasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Command for changing password
/// </summary>
public class ChangePasswordCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}