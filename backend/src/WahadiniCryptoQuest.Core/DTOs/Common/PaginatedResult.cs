namespace WahadiniCryptoQuest.Core.DTOs.Common;

/// <summary>
/// Generic wrapper for paginated API responses with cursor-based pagination
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Collection of items for current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Total count of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Cursor for next page (nullable if no next page)
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// Cursor for previous page (nullable if no previous page)
    /// </summary>
    public string? PreviousCursor { get; set; }

    /// <summary>
    /// Creates an empty paginated result
    /// </summary>
    public PaginatedResult()
    {
    }

    /// <summary>
    /// Creates a paginated result with items and metadata
    /// </summary>
    public PaginatedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Creates a new paginated result by transforming items
    /// </summary>
    public PaginatedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return new PaginatedResult<TResult>
        {
            Items = Items.Select(mapper),
            TotalCount = TotalCount,
            PageNumber = PageNumber,
            PageSize = PageSize,
            NextCursor = NextCursor,
            PreviousCursor = PreviousCursor
        };
    }
}
