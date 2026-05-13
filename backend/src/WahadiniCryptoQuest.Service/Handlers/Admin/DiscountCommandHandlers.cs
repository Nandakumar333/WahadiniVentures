using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;
using WahadiniCryptoQuest.Service.Commands.Admin;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for creating discount codes
/// T136: US5 - Reward System Management
/// </summary>
public class CreateDiscountCodeCommandHandler : IRequestHandler<CreateDiscountCodeCommand, Guid>
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateDiscountCodeCommandHandler> _logger;

    public CreateDiscountCodeCommandHandler(
        IDiscountCodeRepository discountCodeRepository,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ILogger<CreateDiscountCodeCommandHandler> logger)
    {
        _discountCodeRepository = discountCodeRepository;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateDiscountCodeCommand request, CancellationToken cancellationToken)
    {
        // Check for code uniqueness
        var existingCode = await _discountCodeRepository.GetByCodeAsync(request.Data.Code);
        if (existingCode != null)
        {
            throw new InvalidOperationException($"Discount code '{request.Data.Code}' already exists");
        }

        // Validate expiration date
        if (request.Data.ExpirationDate.HasValue && request.Data.ExpirationDate.Value <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Expiration date must be in the future");
        }

        // Validate discount percentage (0-100)
        if (request.Data.DiscountPercentage < 0 || request.Data.DiscountPercentage > 100)
        {
            throw new InvalidOperationException("Discount percentage must be between 0 and 100");
        }

        var discountCode = new DiscountCode
        {
            Code = request.Data.Code,
            DiscountPercentage = request.Data.DiscountPercentage,
            RequiredPoints = request.Data.RequiredPoints,
            ExpiryDate = request.Data.ExpirationDate,
            UsageLimit = request.Data.UsageLimit,
            UsageCount = 0,
            IsActive = true,
            CreatedBy = request.AdminUserId
        };

        await _discountCodeRepository.AddAsync(discountCode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit entry
        await _auditLogService.LogActionAsync(
            adminUserId: request.AdminUserId,
            actionType: "CreateDiscountCode",
            resourceType: "DiscountCode",
            resourceId: discountCode.Id.ToString(),
            beforeValue: null,
            afterValue: System.Text.Json.JsonSerializer.Serialize(new
            {
                discountCode.Code,
                discountCode.DiscountPercentage,
                discountCode.RequiredPoints,
                discountCode.ExpiryDate,
                discountCode.UsageLimit
            }),
            ipAddress: "system",
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Discount code created: {Code} by admin {AdminId}", discountCode.Code, request.AdminUserId);

        return discountCode.Id;
    }
}

/// <summary>
/// Handler for adjusting user points
/// T140: US5 - Reward System Management
/// </summary>
public class AdjustPointsCommandHandler : IRequestHandler<AdjustPointsCommand, int>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdjustPointsCommandHandler> _logger;

    public AdjustPointsCommandHandler(
        IUserRepository userRepository,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<AdjustPointsCommandHandler> logger)
    {
        _userRepository = userRepository;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> Handle(AdjustPointsCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User not found: {request.UserId}");
        }

        var previousBalance = user.CurrentPoints;
        var newBalance = previousBalance + request.AdjustmentAmount;

        // Validate final balance is not negative
        if (newBalance < 0)
        {
            throw new InvalidOperationException($"Point adjustment would result in negative balance. Current: {previousBalance}, Adjustment: {request.AdjustmentAmount}");
        }

        // Update user points using domain methods
        if (request.AdjustmentAmount > 0)
        {
            user.AwardPoints(request.AdjustmentAmount);
        }
        else
        {
            user.DeductPoints(Math.Abs(request.AdjustmentAmount));
        }

        await _userRepository.UpdateAsync(user);

        // Create point adjustment record using factory method
        PointAdjustment.Create(
            userId: request.UserId,
            previousBalance: previousBalance,
            adjustmentAmount: request.AdjustmentAmount,
            reason: request.Reason,
            adminUserId: request.AdminUserId
        );

        // Note: Assuming PointAdjustment repository exists or we use DbContext directly
        // For now, we'll log in audit instead
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification to user
        var notificationMessage = request.AdjustmentAmount > 0
            ? $"You have received {request.AdjustmentAmount} points. Reason: {request.Reason}"
            : $"{Math.Abs(request.AdjustmentAmount)} points have been deducted from your account. Reason: {request.Reason}";

        await _notificationService.CreateInAppNotificationAsync(
            userId: request.UserId,
            type: Core.Enums.NotificationType.PointAdjustment,
            message: notificationMessage
        );

        // Log audit entry
        await _auditLogService.LogActionAsync(
            adminUserId: request.AdminUserId,
            actionType: "AdjustPoints",
            resourceType: "User",
            resourceId: request.UserId.ToString(),
            beforeValue: System.Text.Json.JsonSerializer.Serialize(new { Points = previousBalance }),
            afterValue: System.Text.Json.JsonSerializer.Serialize(new
            {
                Points = newBalance,
                Adjustment = request.AdjustmentAmount,
                Reason = request.Reason
            }),
            ipAddress: "system",
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Points adjusted for user {UserId}: {PreviousBalance} -> {NewBalance} by admin {AdminId}",
            request.UserId, previousBalance, newBalance, request.AdminUserId);

        return newBalance;
    }
}
