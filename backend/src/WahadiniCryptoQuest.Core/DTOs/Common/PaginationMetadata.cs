namespace WahadiniCryptoQuest.Core.DTOs.Common;

/// <summary>
/// Pagination metadata for paginated responses
/// </summary>
public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }

    public PaginationMetadata(int currentPage, int pageSize, int totalCount)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasPrevious = currentPage > 1;
        HasNext = currentPage < TotalPages;
    }
}
