using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace WahadiniCryptoQuest.Service.Services;

/// <summary>
/// High-performance async batch processor with parallel processing
/// Senior Architect Pattern: Producer-Consumer with Channel-based queuing
/// Prevents database/API overload through controlled concurrency
/// </summary>
/// <typeparam name="TInput">Input item type</typeparam>
/// <typeparam name="TOutput">Output result type</typeparam>
public class AsyncBatchProcessor<TInput, TOutput>
{
    private readonly ILogger<AsyncBatchProcessor<TInput, TOutput>> _logger;
    private readonly int _maxDegreeOfParallelism;
    private readonly int _batchSize;
    private readonly SemaphoreSlim _semaphore;

    public AsyncBatchProcessor(
        ILogger<AsyncBatchProcessor<TInput, TOutput>> logger,
        int maxDegreeOfParallelism = 4,
        int batchSize = 500)
    {
        _logger = logger;
        _maxDegreeOfParallelism = maxDegreeOfParallelism;
        _batchSize = batchSize;
        _semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
    }

    /// <summary>
    /// Process items in parallel with controlled concurrency
    /// Uses SemaphoreSlim to limit concurrent database/API calls
    /// </summary>
    public async Task<List<TOutput>> ProcessParallelAsync(
        IEnumerable<TInput> items,
        Func<TInput, CancellationToken, Task<TOutput>> processor,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<TOutput>();
        var itemsList = items.ToList();
        var totalItems = itemsList.Count;

        _logger.LogInformation(
            "Starting parallel processing of {Count} items with max parallelism: {Parallelism}",
            totalItems, _maxDegreeOfParallelism);

        var tasks = itemsList.Select(async item =>
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await processor(item, cancellationToken);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing item");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        _logger.LogInformation("Completed parallel processing of {Count} items", totalItems);
        return results.ToList();
    }

    /// <summary>
    /// Process items in batches with parallel execution within each batch
    /// Optimal for large datasets - prevents memory overflow
    /// </summary>
    public async Task<List<TOutput>> ProcessInBatchesAsync(
        IEnumerable<TInput> items,
        Func<TInput, CancellationToken, Task<TOutput>> processor,
        CancellationToken cancellationToken = default)
    {
        var allResults = new List<TOutput>();
        var itemsList = items.ToList();
        var batches = itemsList
            .Select((item, index) => new { item, index })
            .GroupBy(x => x.index / _batchSize)
            .Select(g => g.Select(x => x.item).ToList())
            .ToList();

        _logger.LogInformation(
            "Processing {TotalItems} items in {BatchCount} batches of max size {BatchSize}",
            itemsList.Count, batches.Count, _batchSize);

        foreach (var (batch, batchIndex) in batches.Select((b, i) => (b, i)))
        {
            _logger.LogInformation("Processing batch {BatchIndex}/{TotalBatches} with {Count} items",
                batchIndex + 1, batches.Count, batch.Count);

            var batchResults = await ProcessParallelAsync(batch, processor, cancellationToken);
            allResults.AddRange(batchResults);

            // Optional: Add delay between batches to prevent overwhelming downstream services
            if (batchIndex < batches.Count - 1)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        _logger.LogInformation("Completed batch processing. Total results: {Count}", allResults.Count);
        return allResults;
    }

    /// <summary>
    /// Process items using Channel-based producer-consumer pattern
    /// Best for streaming scenarios with backpressure handling
    /// </summary>
    public async Task<List<TOutput>> ProcessWithChannelAsync(
        IEnumerable<TInput> items,
        Func<TInput, CancellationToken, Task<TOutput>> processor,
        CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateBounded<TInput>(new BoundedChannelOptions(_batchSize)
        {
            FullMode = BoundedChannelFullMode.Wait // Apply backpressure when full
        });

        var results = new ConcurrentBag<TOutput>();
        var itemsList = items.ToList();

        // Producer: Write items to channel
        var producerTask = Task.Run(async () =>
        {
            try
            {
                foreach (var item in itemsList)
                {
                    await channel.Writer.WriteAsync(item, cancellationToken);
                }
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, cancellationToken);

        // Consumers: Process items from channel with controlled parallelism
        var consumerTasks = Enumerable.Range(0, _maxDegreeOfParallelism)
            .Select(async _ =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        var result = await processor(item, cancellationToken);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing item in channel consumer");
                        throw;
                    }
                }
            })
            .ToArray();

        await Task.WhenAll(producerTask);
        await Task.WhenAll(consumerTasks);

        _logger.LogInformation("Channel processing completed. Total results: {Count}", results.Count);
        return results.ToList();
    }

    /// <summary>
    /// Process with retry logic and exponential backoff
    /// Handles transient failures gracefully
    /// </summary>
    public async Task<TOutput> ProcessWithRetryAsync(
        TInput item,
        Func<TInput, CancellationToken, Task<TOutput>> processor,
        int maxRetries = 3,
        int initialDelayMs = 100,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var delay = initialDelayMs;

        while (true)
        {
            try
            {
                attempt++;
                return await processor(item, cancellationToken);
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex,
                    "Attempt {Attempt}/{MaxRetries} failed. Retrying after {Delay}ms",
                    attempt, maxRetries, delay);

                await Task.Delay(delay, cancellationToken);
                delay *= 2; // Exponential backoff
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "All {MaxRetries} attempts failed", maxRetries);
                throw;
            }
        }
    }
}
