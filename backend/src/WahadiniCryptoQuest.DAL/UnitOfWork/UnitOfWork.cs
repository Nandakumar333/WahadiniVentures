using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.UnitOfWork;

/// <summary>
/// Unit of Work pattern implementation for coordinating multiple repository operations
/// Ensures all operations within a unit of work are committed or rolled back together
/// </summary>
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    private IUserRepository? _userRepository;
    private IEmailVerificationTokenRepository? _emailVerificationTokenRepository;
    private IRefreshTokenRepository? _refreshTokenRepository;
    private IPasswordResetTokenRepository? _passwordResetTokenRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _userRepository ??= 
        new WahadiniCryptoQuest.DAL.Repositories.UserRepository(_context);

    public IEmailVerificationTokenRepository EmailVerificationTokens => 
        _emailVerificationTokenRepository ??= 
        new WahadiniCryptoQuest.DAL.Repositories.EmailVerificationTokenRepository(_context);

    public IRefreshTokenRepository RefreshTokens => 
        _refreshTokenRepository ??= 
        new WahadiniCryptoQuest.DAL.Repositories.RefreshTokenRepository(_context);

    public IPasswordResetTokenRepository PasswordResetTokens => 
        _passwordResetTokenRepository ??= 
        new WahadiniCryptoQuest.DAL.Repositories.PasswordResetTokenRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

/// <summary>
/// Interface for Unit of Work pattern
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IEmailVerificationTokenRepository EmailVerificationTokens { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IPasswordResetTokenRepository PasswordResetTokens { get; }
    Task<int> CompleteAsync();
}
