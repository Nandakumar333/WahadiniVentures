using MediatR;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for updating user role
/// T064: US3 - User Account Management
/// </summary>
public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserRoleCommandHandler(
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

    public async Task<Unit> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        var oldRole = user.Role;

        // Update role using domain method
        user.AssignRole(request.NewRole);

        // Send notification
        var roleChangedMessage = $"Your account role has been updated to {request.NewRole}.";
        if (!string.IsNullOrEmpty(request.Reason))
        {
            roleChangedMessage += $" Reason: {request.Reason}";
        }

        await _notificationService.CreateInAppNotificationAsync(
            request.UserId,
            Core.Enums.NotificationType.AdminAction,
            roleChangedMessage,
            cancellationToken);

        // Log audit entry
        var beforeState = System.Text.Json.JsonSerializer.Serialize(new { role = oldRole });
        var afterState = System.Text.Json.JsonSerializer.Serialize(new { role = request.NewRole, reason = request.Reason });

        await _auditLogService.LogActionAsync(
            request.AdminUserId,
            "UpdateRole",
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
