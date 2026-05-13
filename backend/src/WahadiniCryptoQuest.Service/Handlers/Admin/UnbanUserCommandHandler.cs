using MediatR;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for unbanning a user
/// T066: US3 - User Account Management
/// </summary>
public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public UnbanUserCommandHandler(
        IUserRepository userRepository,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        if (!user.IsBanned)
        {
            throw new InvalidOperationException("User is not banned");
        }

        var oldBanReason = user.BanReason;

        // Unban user using domain method
        user.UnbanAccount();

        // Send notification
        await _notificationService.CreateInAppNotificationAsync(
            request.UserId,
            Core.Enums.NotificationType.AdminAction,
            "Your account ban has been lifted. You can now access the platform.",
            cancellationToken);

        // Log audit entry
        var beforeState = System.Text.Json.JsonSerializer.Serialize(new { isBanned = true, reason = oldBanReason });
        var afterState = System.Text.Json.JsonSerializer.Serialize(new { isBanned = false });

        await _auditLogService.LogActionAsync(
            request.AdminUserId,
            "UnbanUser",
            "User",
            request.UserId.ToString(),
            beforeState,
            afterState,
            string.Empty,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
