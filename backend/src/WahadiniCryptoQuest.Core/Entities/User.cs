using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Enums;

namespace WahadiniCryptoQuest.Core.Entities;

/// <summary>
/// Rich domain model for User entity following Domain-Driven Design principles
/// Contains user identity data and behavior with proper encapsulation
/// </summary>
public class User : BaseEntity
{
    // Private fields for encapsulation
    private string _email = string.Empty;
    private string _passwordHash = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    // Public properties with private setters for encapsulation
    public string Email
    {
        get => _email;
    }

    public string FirstName
    {
        get => _firstName;
    }

    public string LastName
    {
        get => _lastName;
    }

    public string PasswordHash
    {
        get => _passwordHash;
    }

    public bool EmailConfirmed { get; private set; }
    public bool EmailVerified { get; private set; }
    public DateTime? EmailConfirmedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool IsActive { get; private set; } = true;
    public UserRoleEnum Role { get; private set; } = UserRoleEnum.Free;

    // Reward System Properties
    public int CurrentPoints { get; private set; }
    public int TotalPointsEarned { get; private set; }
    public string? ReferralCode { get; private set; }

    /// <summary>
    /// Concurrency token for optimistic locking on point operations
    /// </summary>
    public byte[]? RowVersion { get; protected set; }

    // Admin Dashboard - Ban Management Properties
    /// <summary>
    /// Indicates whether the user account is banned
    /// </summary>
    public bool IsBanned { get; private set; }

    /// <summary>
    /// Reason for banning the user (required when IsBanned = true)
    /// </summary>
    public string? BanReason { get; private set; }

    /// <summary>
    /// UTC timestamp when the user was banned
    /// </summary>
    public DateTime? BannedAt { get; private set; }

    /// <summary>
    /// ID of the admin who banned this user
    /// </summary>
    public Guid? BannedBy { get; private set; }

    /// <summary>
    /// Navigation property to UserRole assignments (many-to-many)
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    /// <summary>
    /// Navigation property for courses created by this user
    /// </summary>
    public virtual ICollection<Course> CreatedCourses { get; set; } = new List<Course>();

    /// <summary>
    /// Navigation property for user's lesson progress
    /// </summary>
    public virtual ICollection<UserProgress> Progress { get; set; } = new List<UserProgress>();

    /// <summary>
    /// Navigation property for user's course enrollments
    /// </summary>
    public virtual ICollection<UserCourseEnrollment> Enrollments { get; set; } = new List<UserCourseEnrollment>();

    /// <summary>
    /// Navigation property for user's task submissions
    /// </summary>
    public virtual ICollection<UserTaskSubmission> TaskSubmissions { get; set; } = new List<UserTaskSubmission>();

    /// <summary>
    /// Navigation property for user's reward transactions
    /// </summary>
    public virtual ICollection<RewardTransaction> RewardTransactions { get; set; } = new List<RewardTransaction>();

    /// <summary>
    /// Navigation property for user's discount redemptions
    /// </summary>
    public virtual ICollection<UserDiscountRedemption> DiscountRedemptions { get; set; } = new List<UserDiscountRedemption>();

    public virtual UserStreak? Streak { get; set; }
    public virtual ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
    public virtual ICollection<ReferralAttribution> ReferralsMade { get; set; } = new List<ReferralAttribution>();
    public virtual ReferralAttribution? ReferredBy { get; set; }

    // Private constructor for Entity Framework
    private User() { }

    /// <summary>
    /// Factory method for creating a new User entity with validation
    /// Follows Domain-Driven Design principles
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="passwordHash">Hashed password</param>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <returns>New User entity</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        // Validate required parameters
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        var user = new User
        {
            Id = Guid.NewGuid(),
            _email = email.Trim(),
            _passwordHash = passwordHash,
            _firstName = firstName.Trim(),
            _lastName = lastName.Trim(),
            EmailConfirmed = false,
            EmailVerified = false,
            IsActive = true,
            Role = UserRoleEnum.Free,
            FailedLoginAttempts = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return user;
    }

    /// <summary>
    /// Domain method to confirm user's email address
    /// </summary>
    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        EmailVerified = true;
        EmailConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to record successful login and reset failed attempts
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to increment failed login attempts
    /// </summary>
    public void IncrementFailedLoginAttempts()
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to lock user account for specified duration
    /// </summary>
    /// <param name="lockoutDuration">Duration to lock the account</param>
    public void LockAccount(TimeSpan lockoutDuration)
    {
        LockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to unlock user account and reset failed attempts
    /// </summary>
    public void UnlockAccount()
    {
        LockoutEnd = null;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to check if user account is currently locked
    /// </summary>
    /// <returns>True if account is locked, false otherwise</returns>
    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to update user profile information
    /// </summary>
    /// <param name="firstName">New first name</param>
    /// <param name="lastName">New last name</param>
    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        _firstName = firstName.Trim();
        _lastName = lastName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to assign a role to the user
    /// </summary>
    /// <param name="role">The role to assign</param>
    public void AssignRole(UserRoleEnum role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to update user password
    /// </summary>
    /// <param name="hashedPassword">New hashed password</param>
    public void UpdatePassword(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(hashedPassword));

        _passwordHash = hashedPassword;
        UpdatedAt = DateTime.UtcNow;

        // Reset lockout on successful password change
        if (IsLockedOut())
        {
            UnlockAccount();
        }
    }

    /// <summary>
    /// Domain method to deactivate user account
    /// </summary>
    public void DeactivateAccount()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to reactivate user account
    /// </summary>
    public void ReactivateAccount()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to upgrade user role (e.g., Free to Premium)
    /// </summary>
    /// <param name="newRole">New user role</param>
    public void UpgradeRole(UserRoleEnum newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Computed property for user's full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Domain method to award points to user (for reward system)
    /// Includes optimistic concurrency validation
    /// </summary>
    /// <param name="amount">Points to award (must be positive)</param>
    public void AwardPoints(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Award amount must be positive", nameof(amount));
        if (amount > 100000)
            throw new ArgumentException("Award amount exceeds maximum limit", nameof(amount));

        CurrentPoints += amount;
        TotalPointsEarned += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to deduct points from user (for redemptions/penalties)
    /// Includes optimistic concurrency validation and balance checks
    /// </summary>
    /// <param name="amount">Points to deduct (must be positive)</param>
    public void DeductPoints(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deduction amount must be positive", nameof(amount));
        if (CurrentPoints < amount)
            throw new InvalidOperationException($"Insufficient balance: {CurrentPoints} < {amount}");

        CurrentPoints -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to generate and set referral code (6 uppercase alphanumeric)
    /// </summary>
    public void GenerateReferralCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        ReferralCode = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to ban a user account
    /// </summary>
    /// <param name="reason">Reason for banning (required)</param>
    /// <param name="adminUserId">ID of admin who is banning the user</param>
    public void BanAccount(string reason, Guid adminUserId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Ban reason is required", nameof(reason));

        if (reason.Length > 500)
            throw new ArgumentException("Ban reason cannot exceed 500 characters", nameof(reason));

        if (adminUserId == Guid.Empty)
            throw new ArgumentException("AdminUserId cannot be empty", nameof(adminUserId));

        if (IsBanned)
            throw new InvalidOperationException("User is already banned");

        IsBanned = true;
        BanReason = reason;
        BannedAt = DateTime.UtcNow;
        BannedBy = adminUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Domain method to unban a user account
    /// </summary>
    public void UnbanAccount()
    {
        if (!IsBanned)
            throw new InvalidOperationException("User is not currently banned");

        IsBanned = false;
        BanReason = null;
        BannedAt = null;
        BannedBy = null;
        UpdatedAt = DateTime.UtcNow;
    }
}