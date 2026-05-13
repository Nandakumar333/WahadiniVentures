using System.Text;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.DTOs.Common;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Queries.Reward;

/// <summary>
/// Handler for retrieving paginated transaction history with cursor-based pagination
/// </summary>
public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, PaginatedResult<TransactionDto>>
{
    private readonly IRewardTransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private const int MaxPageSize = 100;
    private const int CursorExpirationHours = 1;

    public GetTransactionHistoryQueryHandler(
        IRewardTransactionRepository transactionRepository,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TransactionDto>> Handle(
        GetTransactionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Validate and limit page size
        var pageSize = Math.Min(request.PageSize, MaxPageSize);
        if (pageSize <= 0) pageSize = 20;

        // Decode cursor if provided
        DateTime? cursorTimestamp = null;
        Guid? cursorTransactionId = null;

        if (!string.IsNullOrEmpty(request.Cursor))
        {
            try
            {
                var (timestamp, transactionId, createdAt) = DecodeCursor(request.Cursor);

                // Validate cursor expiration (1 hour)
                if (DateTime.UtcNow - createdAt > TimeSpan.FromHours(CursorExpirationHours))
                {
                    throw new InvalidOperationException("Cursor has expired. Please start a new query.");
                }

                cursorTimestamp = timestamp;
                cursorTransactionId = transactionId;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new ArgumentException("Invalid cursor format", nameof(request.Cursor), ex);
            }
        }

        // Build query with filters
        var query = _transactionRepository.GetQueryable()
            .Where(t => t.UserId == request.UserId);

        // Apply transaction type filter if provided
        if (!string.IsNullOrEmpty(request.TransactionType) &&
            Enum.TryParse<TransactionType>(request.TransactionType, true, out var transactionType))
        {
            query = query.Where(t => t.TransactionType == transactionType);
        }

        // Apply cursor-based pagination (descending by CreatedAt)
        if (cursorTimestamp.HasValue && cursorTransactionId.HasValue)
        {
            query = query.Where(t =>
                t.CreatedAt < cursorTimestamp.Value ||
                (t.CreatedAt == cursorTimestamp.Value && t.Id.CompareTo(cursorTransactionId.Value) < 0));
        }

        // Order by CreatedAt descending (newest first), then by Id for consistency
        query = query.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id);

        // Get total count (for metadata)
        var totalCount = await _transactionRepository.GetQueryable()
            .Where(t => t.UserId == request.UserId)
            .CountAsync(cancellationToken);

        // Fetch page + 1 to determine if there's a next page
        var transactions = await query
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        // Determine if there's a next page
        var hasNextPage = transactions.Count > pageSize;
        var itemsToReturn = hasNextPage ? transactions.Take(pageSize).ToList() : transactions;

        // Map to DTOs
        var transactionDtos = _mapper.Map<List<TransactionDto>>(itemsToReturn);

        // Generate next cursor if there are more pages
        string? nextCursor = null;
        if (hasNextPage && itemsToReturn.Any())
        {
            var lastItem = itemsToReturn.Last();
            nextCursor = EncodeCursor(lastItem.CreatedAt, lastItem.Id);
        }

        // Calculate page number (approximate, as cursor-based pagination doesn't have absolute page numbers)
        var currentPosition = cursorTimestamp.HasValue
            ? await _transactionRepository.GetQueryable()
                .Where(t => t.UserId == request.UserId && t.CreatedAt >= cursorTimestamp.Value)
                .CountAsync(cancellationToken)
            : 0;

        var approximatePageNumber = (currentPosition / pageSize) + 1;

        return new PaginatedResult<TransactionDto>
        {
            Items = transactionDtos,
            TotalCount = totalCount,
            PageNumber = approximatePageNumber,
            PageSize = pageSize,
            NextCursor = nextCursor,
            PreviousCursor = request.Cursor // Original cursor for going back
        };
    }

    /// <summary>
    /// Encodes cursor with timestamp, transaction ID, and creation time for expiration validation
    /// Format: timestamp|transactionId|cursorCreatedAt (Base64)
    /// </summary>
    private static string EncodeCursor(DateTime timestamp, Guid transactionId)
    {
        var cursorCreatedAt = DateTime.UtcNow;
        var cursorData = $"{timestamp:O}|{transactionId}|{cursorCreatedAt:O}";
        var bytes = Encoding.UTF8.GetBytes(cursorData);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Decodes cursor to extract timestamp, transaction ID, and cursor creation time
    /// </summary>
    private static (DateTime timestamp, Guid transactionId, DateTime cursorCreatedAt) DecodeCursor(string cursor)
    {
        var bytes = Convert.FromBase64String(cursor);
        var cursorData = Encoding.UTF8.GetString(bytes);
        var parts = cursorData.Split('|');

        if (parts.Length != 3)
            throw new FormatException("Invalid cursor format");

        var timestamp = DateTime.Parse(parts[0]);
        var transactionId = Guid.Parse(parts[1]);
        var cursorCreatedAt = DateTime.Parse(parts[2]);

        return (timestamp, transactionId, cursorCreatedAt);
    }
}
