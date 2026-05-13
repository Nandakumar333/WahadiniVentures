using WahadiniCryptoQuest.Service.BackgroundJobs;

namespace WahadiniCryptoQuest.API.BackgroundServices;

/// <summary>
/// Background hosted service that runs DeduplicationCleanupJob daily at 2 AM UTC
/// </summary>
public class DeduplicationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeduplicationCleanupService> _logger;
    private static readonly TimeSpan TargetTime = new TimeSpan(2, 0, 0); // 2 AM UTC

    public DeduplicationCleanupService(
        IServiceProvider serviceProvider,
        ILogger<DeduplicationCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Deduplication Cleanup Service started. Will run daily at 2 AM UTC.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = CalculateNextRunTime(now);
                var delay = nextRun - now;

                _logger.LogInformation(
                    "Next deduplication cleanup scheduled for {NextRun} UTC (in {Hours}h {Minutes}m)",
                    nextRun,
                    (int)delay.TotalHours,
                    delay.Minutes);

                // Wait until the scheduled time
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await RunCleanupAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
                _logger.LogInformation("Deduplication Cleanup Service is shutting down.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Deduplication Cleanup Service main loop.");
                // Wait a bit before retrying to avoid tight error loops
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                var cleanupJob = scope.ServiceProvider.GetRequiredService<DeduplicationCleanupJob>();
                await cleanupJob.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during deduplication cleanup job execution.");
            }
        }
    }

    private static DateTime CalculateNextRunTime(DateTime now)
    {
        var todayRun = now.Date.Add(TargetTime);

        // If today's run time has passed, schedule for tomorrow
        if (now >= todayRun)
        {
            return todayRun.AddDays(1);
        }

        return todayRun;
    }
}
