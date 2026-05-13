namespace WahadiniCryptoQuest.Core.Exceptions;

/// <summary>
/// Base exception for reward system-related errors
/// </summary>
public class RewardException : Exception
{
    public string ErrorCode { get; }

    public RewardException(string message, string errorCode = "REWARD_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public RewardException(string message, Exception innerException, string errorCode = "REWARD_ERROR")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when a user has insufficient points for an operation
/// </summary>
public class InsufficientPointsException : RewardException
{
    public Guid UserId { get; }
    public int CurrentBalance { get; }
    public int RequiredAmount { get; }

    public InsufficientPointsException(Guid userId, int currentBalance, int requiredAmount)
        : base($"User {userId} has insufficient points. Current: {currentBalance}, Required: {requiredAmount}", "INSUFFICIENT_POINTS")
    {
        UserId = userId;
        CurrentBalance = currentBalance;
        RequiredAmount = requiredAmount;
    }
}

/// <summary>
/// Exception thrown when a transaction fails due to concurrency conflicts
/// </summary>
public class TransactionConcurrencyException : RewardException
{
    public Guid UserId { get; }
    public int RetryCount { get; }

    public TransactionConcurrencyException(Guid userId, int retryCount)
        : base($"Transaction for user {userId} failed after {retryCount} retry attempts due to concurrency conflicts", "TRANSACTION_CONCURRENCY_ERROR")
    {
        UserId = userId;
        RetryCount = retryCount;
    }
}

/// <summary>
/// Exception thrown when a duplicate transaction is detected
/// </summary>
public class DuplicateTransactionException : RewardException
{
    public string ReferenceId { get; }
    public string ReferenceType { get; }

    public DuplicateTransactionException(string referenceId, string referenceType)
        : base($"Duplicate transaction detected for reference {referenceType}:{referenceId}", "DUPLICATE_TRANSACTION")
    {
        ReferenceId = referenceId;
        ReferenceType = referenceType;
    }
}
