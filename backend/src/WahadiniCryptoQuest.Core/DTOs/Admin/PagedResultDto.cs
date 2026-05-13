namespace WahadiniCryptoQuest.Core.DTOs.Admin;

/// <summary>
/// Generic paginated result DTO for admin endpoints.
/// Wraps data with pagination metadata.
/// </summary>
public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
