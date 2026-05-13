using MediatR;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for banning a user
/// T065: US3 - User Account Management
/// </summary>
public class BanUserCommandHandler : IRequestHandler<BanUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public BanUserCommandHandler(
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

    public async Task<Unit> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        if (user.IsBanned)
        {
            throw new InvalidOperationException("User is already banned");
        }

        // Ban user using domain method
        user.BanAccount(request.Reason, request.AdminUserId);

        // Send notification
        await _notificationService.CreateInAppNotificationAsync(
            request.UserId,
            Core.Enums.NotificationType.AdminAction,
            $"Your account has been banned. Reason: {request.Reason}",
            cancellationToken);

        // Log audit entry
        var beforeState = System.Text.Json.JsonSerializer.Serialize(new { isBanned = false });
        var afterState = System.Text.Json.JsonSerializer.Serialize(new { isBanned = true, reason = request.Reason });

        await _auditLogService.LogActionAsync(
            request.AdminUserId,
            "BanUser",
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
