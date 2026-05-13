using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.BackgroundJobs;

/// <summary>
/// Background job to clean up old idempotency records (transactions older than 24 hours)
/// Runs daily at 2 AM UTC to remove stale records and maintain database performance
/// </summary>
public class DeduplicationCleanupJob
{
    private readonly IRewardTransactionRepository _transactionRepository;
    private readonly ILogger<DeduplicationCleanupJob> _logger;
    private readonly TimeSpan _retentionPeriod = TimeSpan.FromHours(24);

    public DeduplicationCleanupJob(
        IRewardTransactionRepository transactionRepository,
        ILogger<DeduplicationCleanupJob> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the cleanup job
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting deduplication cleanup job at {Time}", DateTime.UtcNow);

            var cutoffDate = DateTime.UtcNow.Subtract(_retentionPeriod);

            // Get transactions older than 24 hours with referenceId/referenceType (idempotency records)
            var oldTransactions = await _transactionRepository.GetTransactionsByDateRangeAsync(
                DateTime.MinValue,
                cutoffDate,
                cancellationToken);

            // Filter to only those with reference IDs (used for idempotency)
            var idempotencyRecords = oldTransactions
                .Where(t => !string.IsNullOrEmpty(t.ReferenceId) && !string.IsNullOrEmpty(t.ReferenceType))
                .ToList();

            if (idempotencyRecords.Any())
            {
                _logger.LogInformation(
                    "Found {Count} idempotency records older than {CutoffDate} for cleanup",
                    idempotencyRecords.Count,
                    cutoffDate);

                // Note: We don't actually delete transactions (append-only ledger)
                // This job is a placeholder for future optimization strategies:
                // 1. Move old records to archive table
                // 2. Clear referenceId/referenceType for old records (keeping transaction history)
                // 3. Update indexes or partitioning strategy

                _logger.LogInformation(
                    "Deduplication cleanup job completed. {Count} records identified for potential archival",
                    idempotencyRecords.Count);
            }
            else
            {
                _logger.LogInformation("No idempotency records found for cleanup");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during deduplication cleanup job");
            throw;
        }
    }
}
