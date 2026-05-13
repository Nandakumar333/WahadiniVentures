using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Services;

public class RewardService : IRewardService
{
    private readonly IRewardTransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RewardService> _logger;

    public RewardService(
        IRewardTransactionRepository transactionRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<RewardService> logger)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> AwardPointsAsync(
        Guid userId,
        int amount,
        TransactionType type,
        string description,
        string? referenceId = null,
        string? referenceType = null,
        Guid? adminUserId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Attempting to award {Amount} points to user {UserId}. Type: {Type}, Description: {Description}, Reference: {ReferenceId}/{ReferenceType}, AdminUser: {AdminUserId}",
            amount, userId, type, description, referenceId, referenceType, adminUserId);

        // Idempotency check: If referenceId and referenceType provided, check for existing transaction
        if (!string.IsNullOrEmpty(referenceId) && !string.IsNullOrEmpty(referenceType))
        {
            var existingTransaction = await _transactionRepository.GetByReferenceAsync(
                userId,
                referenceId,
                referenceType,
                cancellationToken);

            if (existingTransaction != null)
            {
                _logger.LogInformation(
                    "Idempotent request detected for user {UserId}, reference {ReferenceId}/{ReferenceType}. Returning existing transaction {TransactionId}",
                    userId, referenceId, referenceType, existingTransaction.Id);
                // Already processed, return existing transaction ID (idempotent)
                return existingTransaction.Id;
            }
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogError("User {UserId} not found when attempting to award {Amount} points", userId, amount);
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        var previousBalance = user.CurrentPoints;

        // Use domain method to update user state
        user.AwardPoints(amount);

        // Create immutable transaction record
        var transaction = RewardTransaction.Create(
            userId,
            amount,
            type,
            description,
            user.CurrentPoints,
            referenceId,
            referenceType,
            adminUserId
        );

        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully awarded {Amount} points to user {UserId}. Transaction ID: {TransactionId}, Previous balance: {PreviousBalance}, New balance: {NewBalance}",
            amount, userId, transaction.Id, previousBalance, user.CurrentPoints);

        return transaction.Id;
    }

    public async Task<Guid> DeductPointsAsync(
        Guid userId,
        int amount,
        TransactionType type,
        string description,
        string? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Attempting to deduct {Amount} points from user {UserId}. Type: {Type}, Description: {Description}",
            amount, userId, type, description);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogError("User {UserId} not found when attempting to deduct {Amount} points", userId, amount);
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        var previousBalance = user.CurrentPoints;

        // Use domain method to update user state (includes validation)
        try
        {
            user.DeductPoints(amount);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Failed to deduct {Amount} points from user {UserId}. Insufficient balance: {CurrentBalance}. Error: {Error}",
                amount, userId, previousBalance, ex.Message);
            throw;
        }

        // Create immutable transaction record (negative amount for deduction)
        var transaction = RewardTransaction.Create(
            userId,
            -amount,
            type,
            description,
            user.CurrentPoints,
            referenceId,
            referenceType
        );

        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully deducted {Amount} points from user {UserId}. Transaction ID: {TransactionId}, Previous balance: {PreviousBalance}, New balance: {NewBalance}",
            amount, userId, transaction.Id, previousBalance, user.CurrentPoints);

        return transaction.Id;
    }

    public async Task<int> GetUserBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        return user.CurrentPoints;
    }
}
