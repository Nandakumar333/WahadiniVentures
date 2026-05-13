using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for retrieving detailed user information
/// T063: US3 - User Account Management
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<Queries.Admin.GetUserByIdQuery, UserDetailDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserTaskSubmissionRepository _taskSubmissionRepository;
    private readonly IUserCourseEnrollmentRepository _enrollmentRepository;

    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        ISubscriptionRepository subscriptionRepository,
        IUserTaskSubmissionRepository taskSubmissionRepository,
        IUserCourseEnrollmentRepository enrollmentRepository)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _taskSubmissionRepository = taskSubmissionRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<UserDetailDto?> Handle(Queries.Admin.GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return null;
        }

        // Get subscription info
        var activeSubscription = (await _subscriptionRepository.FindAsync(s =>
            s.UserId == request.UserId &&
            s.Status == SubscriptionStatus.Active)).FirstOrDefault();

        // Get activity stats
        var enrollments = await _enrollmentRepository.FindAsync(e => e.UserId == request.UserId);
        var taskSubmissions = await _taskSubmissionRepository.FindAsync(t => t.UserId == request.UserId);

        var completedTasks = taskSubmissions.Count(t => t.Status == SubmissionStatus.Approved);
        var pendingTasks = taskSubmissions.Count(t => t.Status == SubmissionStatus.Pending);

        return new UserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            CurrentPoints = user.CurrentPoints,
            TotalPointsEarned = user.TotalPointsEarned,
            EmailConfirmed = user.EmailConfirmed,
            EmailVerified = user.EmailVerified,
            IsActive = user.IsActive,
            IsBanned = user.IsBanned,
            BanReason = user.BanReason,
            BannedAt = user.BannedAt,
            BannedBy = user.BannedBy,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LockoutEnd = user.LockoutEnd,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            HasActiveSubscription = activeSubscription != null,
            SubscriptionStatus = activeSubscription?.Status.ToString(),
            SubscriptionStartDate = activeSubscription?.CreatedAt,
            SubscriptionEndDate = activeSubscription?.CancelledAt,
            EnrolledCoursesCount = enrollments.Count,
            CompletedTasksCount = completedTasks,
            PendingTasksCount = pendingTasks
        };
    }
}
