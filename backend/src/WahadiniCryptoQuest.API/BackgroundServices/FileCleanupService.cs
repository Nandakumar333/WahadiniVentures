using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.API.BackgroundServices;

public class FileCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FileCleanupService> _logger;

    public FileCleanupService(IServiceProvider serviceProvider, ILogger<FileCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("File Cleanup Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupRejectedFilesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during file cleanup.");
            }

            // Run daily
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task CleanupRejectedFilesAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            var rejectedSubmissions = await dbContext.UserTaskSubmissions
                .Where(s => s.Status == SubmissionStatus.Rejected && 
                            s.ReviewedAt < cutoffDate &&
                            s.Task.TaskType == TaskType.Screenshot)
                .Include(s => s.Task)
                .ToListAsync(cancellationToken);

            foreach (var submission in rejectedSubmissions)
            {
                // Parse SubmissionData to get file path
                // Assuming JSON: { "filePath": "...", ... }
                try 
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(submission.SubmissionData);
                    if (doc.RootElement.TryGetProperty("filePath", out var filePathElement))
                    {
                        var filePath = filePathElement.GetString();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            await fileStorage.DeleteFileAsync(filePath, cancellationToken);
                            _logger.LogInformation("Deleted file for rejected submission {SubmissionId}", submission.Id);
                            
                            // Optional: Clear the file path from DB to prevent retry
                            // submission.SubmissionData = ... 
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse submission data for {SubmissionId}", submission.Id);
                }
            }
        }
    }
}
