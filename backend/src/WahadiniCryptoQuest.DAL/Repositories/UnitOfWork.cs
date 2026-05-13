using Microsoft.EntityFrameworkCore.Storage;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Unit of Work implementation that coordinates repository operations and manages transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // Lazy-initialized repository fields
    private ICategoryRepository? _categories;
    private ICourseRepository? _courses;
    private ILessonRepository? _lessons;
    private ILearningTaskRepository? _learningTasks;
    private IUserProgressRepository? _userProgress;
    private IUserCourseEnrollmentRepository? _enrollments;
    private IUserTaskSubmissionRepository? _taskSubmissions;
    private IRewardTransactionRepository? _rewardTransactions;
    private IUserStreakRepository? _userStreaks;
    private IUserAchievementRepository? _userAchievements;
    private IReferralAttributionRepository? _referralAttributions;
    private IDiscountCodeRepository? _discountCodes;
    private IUserDiscountRedemptionRepository? _userDiscountRedemptions;
    private IRefreshTokenRepository? _refreshTokens;
    private IUserRepository? _users;
    private IEmailVerificationTokenRepository? _emailVerificationTokens;
    private IPasswordResetTokenRepository? _passwordResetTokens;
    private IRoleRepository? _roles;
    private IUserRoleRepository? _userRoles;
    private IPermissionRepository? _permissions;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the Category repository with lazy initialization
    /// </summary>
    public ICategoryRepository Categories
    {
        get
        {
            _categories ??= new CategoryRepository(_context);
            return _categories;
        }
    }

    /// <summary>
    /// Gets the Course repository with lazy initialization
    /// </summary>
    public ICourseRepository Courses
    {
        get
        {
            _courses ??= new CourseRepository(_context);
            return _courses;
        }
    }

    /// <summary>
    /// Gets the Lesson repository with lazy initialization
    /// </summary>
    public ILessonRepository Lessons
    {
        get
        {
            _lessons ??= new LessonRepository(_context);
            return _lessons;
        }
    }

    /// <summary>
    /// Gets the LearningTask repository with lazy initialization
    /// </summary>
    public ILearningTaskRepository LearningTasks
    {
        get
        {
            _learningTasks ??= new LearningTaskRepository(_context);
            return _learningTasks;
        }
    }

    /// <summary>
    /// Gets the UserProgress repository with lazy initialization
    /// </summary>
    public IUserProgressRepository UserProgress
    {
        get
        {
            _userProgress ??= new UserProgressRepository(_context);
            return _userProgress;
        }
    }

    /// <summary>
    /// Gets the UserCourseEnrollment repository with lazy initialization
    /// </summary>
    public IUserCourseEnrollmentRepository Enrollments
    {
        get
        {
            _enrollments ??= new UserCourseEnrollmentRepository(_context);
            return _enrollments;
        }
    }

    /// <summary>
    /// Gets the UserTaskSubmission repository with lazy initialization
    /// </summary>
    public IUserTaskSubmissionRepository TaskSubmissions
    {
        get
        {
            _taskSubmissions ??= new UserTaskSubmissionRepository(_context);
            return _taskSubmissions;
        }
    }

    /// <summary>
    /// Gets the RewardTransaction repository with lazy initialization
    /// </summary>
    public IRewardTransactionRepository RewardTransactions
    {
        get
        {
            _rewardTransactions ??= new RewardTransactionRepository(_context);
            return _rewardTransactions;
        }
    }

    /// <summary>
    /// Gets the UserStreak repository with lazy initialization
    /// </summary>
    public IUserStreakRepository UserStreaks
    {
        get
        {
            _userStreaks ??= new UserStreakRepository(_context);
            return _userStreaks;
        }
    }

    /// <summary>
    /// Gets the UserAchievement repository with lazy initialization
    /// </summary>
    public IUserAchievementRepository UserAchievements
    {
        get
        {
            _userAchievements ??= new UserAchievementRepository(_context);
            return _userAchievements;
        }
    }

    /// <summary>
    /// Gets the ReferralAttribution repository with lazy initialization
    /// </summary>
    public IReferralAttributionRepository ReferralAttributions
    {
        get
        {
            _referralAttributions ??= new ReferralAttributionRepository(_context);
            return _referralAttributions;
        }
    }

    /// <summary>
    /// Gets the DiscountCode repository with lazy initialization
    /// </summary>
    public IDiscountCodeRepository DiscountCodes
    {
        get
        {
            _discountCodes ??= new DiscountCodeRepository(_context);
            return _discountCodes;
        }
    }

    /// <summary>
    /// Gets the UserDiscountRedemption repository with lazy initialization
    /// </summary>
    public IUserDiscountRedemptionRepository UserDiscountRedemptions
    {
        get
        {
            _userDiscountRedemptions ??= new UserDiscountRedemptionRepository(_context);
            return _userDiscountRedemptions;
        }
    }

    /// <summary>
    /// Gets the RefreshToken repository with lazy initialization
    /// </summary>
    public IRefreshTokenRepository RefreshTokens
    {
        get
        {
            _refreshTokens ??= new RefreshTokenRepository(_context);
            return _refreshTokens;
        }
    }

    /// <summary>
    /// Gets the User repository with lazy initialization
    /// </summary>
    public IUserRepository Users
    {
        get
        {
            _users ??= new UserRepository(_context);
            return _users;
        }
    }

    /// <summary>
    /// Gets the EmailVerificationToken repository with lazy initialization
    /// </summary>
    public IEmailVerificationTokenRepository EmailVerificationTokens
    {
        get
        {
            _emailVerificationTokens ??= new EmailVerificationTokenRepository(_context);
            return _emailVerificationTokens;
        }
    }

    /// <summary>
    /// Gets the PasswordResetToken repository with lazy initialization
    /// </summary>
    public IPasswordResetTokenRepository PasswordResetTokens
    {
        get
        {
            _passwordResetTokens ??= new PasswordResetTokenRepository(_context);
            return _passwordResetTokens;
        }
    }

    /// <summary>
    /// Gets the Role repository with lazy initialization
    /// </summary>
    public IRoleRepository Roles
    {
        get
        {
            _roles ??= new RoleRepository(_context);
            return _roles;
        }
    }

    /// <summary>
    /// Gets the UserRole repository with lazy initialization
    /// </summary>
    public IUserRoleRepository UserRoles
    {
        get
        {
            _userRoles ??= new UserRoleRepository(_context);
            return _userRoles;
        }
    }

    /// <summary>
    /// Gets the Permission repository with lazy initialization
    /// </summary>
    public IPermissionRepository Permissions
    {
        get
        {
            _permissions ??= new PermissionRepository(_context);
            return _permissions;
        }
    }

    /// <summary>
    /// Saves all changes made in this unit of work to the database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new transaction
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return; // No transaction to rollback
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Disposes the Unit of Work and its resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentTransaction?.Dispose();
            _context?.Dispose();
        }
    }
}
