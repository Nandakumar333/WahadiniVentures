using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Auth;
using WahadiniCryptoQuest.Core.DTOs;

namespace WahadiniCryptoQuest.Service.Queries.Auth;

/// <summary>
/// Query to get user by ID
/// </summary>
public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid UserId { get; set; }
}

/// <summary>
/// Query to get user by email
/// </summary>
public class GetUserByEmailQuery : IRequest<UserDto?>
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Query to validate refresh token
/// </summary>
public class ValidateRefreshTokenQuery : IRequest<bool>
{
    public string RefreshToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}