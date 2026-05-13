namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Unit of Work interface that coordinates repository operations and manages transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the Category repository
    /// </summary>
    ICategoryRepository Categories { get; }

    /// <summary>
    /// Gets the Course repository
    /// </summary>
    ICourseRepository Courses { get; }

    /// <summary>
    /// Gets the Lesson repository
    /// </summary>
    ILessonRepository Lessons { get; }

    /// <summary>
    /// Gets the LearningTask repository
    /// </summary>
    ILearningTaskRepository LearningTasks { get; }

    /// <summary>
    /// Gets the UserProgress repository
    /// </summary>
    IUserProgressRepository UserProgress { get; }

    /// <summary>
    /// Gets the UserCourseEnrollment repository
    /// </summary>
    IUserCourseEnrollmentRepository Enrollments { get; }

    /// <summary>
    /// Gets the UserTaskSubmission repository
    /// </summary>
    IUserTaskSubmissionRepository TaskSubmissions { get; }

    /// <summary>
    /// Gets the RewardTransaction repository
    /// </summary>
    IRewardTransactionRepository RewardTransactions { get; }

    /// <summary>
    /// Gets the UserStreak repository
    /// </summary>
    IUserStreakRepository UserStreaks { get; }

    /// <summary>
    /// Gets the UserAchievement repository
    /// </summary>
    IUserAchievementRepository UserAchievements { get; }

    /// <summary>
    /// Gets the ReferralAttribution repository
    /// </summary>
    IReferralAttributionRepository ReferralAttributions { get; }

    /// <summary>
    /// Gets the DiscountCode repository
    /// </summary>
    IDiscountCodeRepository DiscountCodes { get; }

    /// <summary>
    /// Gets the UserDiscountRedemption repository
    /// </summary>
    IUserDiscountRedemptionRepository UserDiscountRedemptions { get; }

    /// <summary>
    /// Gets the RefreshToken repository
    /// </summary>
    IRefreshTokenRepository RefreshTokens { get; }

    /// <summary>
    /// Gets the User repository
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Gets the EmailVerificationToken repository
    /// </summary>
    IEmailVerificationTokenRepository EmailVerificationTokens { get; }

    /// <summary>
    /// Gets the PasswordResetToken repository
    /// </summary>
    IPasswordResetTokenRepository PasswordResetTokens { get; }

    /// <summary>
    /// Gets the Role repository
    /// </summary>
    IRoleRepository Roles { get; }

    /// <summary>
    /// Gets the UserRole repository
    /// </summary>
    IUserRoleRepository UserRoles { get; }

    /// <summary>
    /// Gets the Permission repository
    /// </summary>
    IPermissionRepository Permissions { get; }

    /// <summary>
    /// Saves all changes made in this unit of work to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transaction object</returns>
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
