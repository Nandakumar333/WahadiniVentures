using AutoMapper;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Notifications;

/// <summary>
/// Service for managing in-app user notifications.
/// Creates, retrieves, and marks notifications as read.
/// T014: InAppNotificationService with UserNotification creation and retrieval
/// </summary>
public class InAppNotificationService : INotificationService
{
    private readonly IUserNotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<InAppNotificationService> _logger;
    private readonly EmailNotificationService _emailNotificationService;

    public InAppNotificationService(
        IUserNotificationRepository notificationRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<InAppNotificationService> logger,
        EmailNotificationService emailNotificationService)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _emailNotificationService = emailNotificationService;
    }

    public async Task<UserNotificationDto> CreateInAppNotificationAsync(
        Guid userId,
        NotificationType type,
        string message,
        CancellationToken cancellationToken = default)
    {
        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Attempted to create notification for non-existent user {UserId}", userId);
            throw new ArgumentException($"User with ID {userId} not found", nameof(userId));
        }

        // Create notification using factory method
        var notification = UserNotification.Create(
            userId: userId,
            type: type.ToString(),
            message: message);

        // Persist to database
        await _notificationRepository.AddAsync(notification);

        _logger.LogInformation("In-app notification created for user {UserId}: {Type}", userId, type);

        return _mapper.Map<UserNotificationDto>(notification);
    }

    public async Task<bool> SendEmailAsync(
        Guid userId,
        string subject,
        string templateName,
        Dictionary<string, string> templateData,
        CancellationToken cancellationToken = default)
    {
        return await _emailNotificationService.SendEmailAsync(
            userId, subject, templateName, templateData, cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);

        if (notification == null)
        {
            _logger.LogWarning("Notification {NotificationId} not found", notificationId);
            return false;
        }

        // Authorization check - ensure notification belongs to user
        if (notification.UserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to mark notification {NotificationId} as read, but it belongs to user {OwnerId}",
                userId, notificationId, notification.UserId);
            return false;
        }

        // Mark as read using domain method
        notification.MarkAsRead();

        await _notificationRepository.UpdateAsync(notification);

        _logger.LogInformation("Notification {NotificationId} marked as read by user {UserId}",
            notificationId, userId);

        return true;
    }

    public async Task<PagedResultDto<UserNotificationDto>> GetUnreadNotificationsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _notificationRepository.GetUnreadAsync(
            userId, pageNumber, pageSize, cancellationToken);

        var dtos = _mapper.Map<List<UserNotificationDto>>(items);

        return new PagedResultDto<UserNotificationDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<UserNotificationDto>> GetNotificationsAsync(
        Guid userId,
        bool? isRead = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _notificationRepository.GetByUserIdAsync(
            userId, isRead, pageNumber, pageSize, cancellationToken);

        var dtos = _mapper.Map<List<UserNotificationDto>>(items);

        return new PagedResultDto<UserNotificationDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task<int> MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
    }
}
