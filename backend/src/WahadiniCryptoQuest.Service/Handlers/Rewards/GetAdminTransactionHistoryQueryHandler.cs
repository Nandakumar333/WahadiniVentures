using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.DTOs.Common;
using WahadiniCryptoQuest.Core.DTOs.Reward;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Handlers.Rewards;

/// <summary>
/// Handler for getting admin-enriched transaction history with audit trail
/// </summary>
public class GetAdminTransactionHistoryQueryHandler
    : IRequestHandler<Queries.Rewards.GetAdminTransactionHistoryQuery, PaginatedResult<AdminTransactionHistoryDto>>
{
    private readonly IRewardTransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetAdminTransactionHistoryQueryHandler(
        IRewardTransactionRepository transactionRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<AdminTransactionHistoryDto>> Handle(
        Queries.Rewards.GetAdminTransactionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Verify user exists
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");
        }

        // Parse cursor if provided
        DateTime? cursorDate = null;
        Guid? cursorId = null;
        if (!string.IsNullOrEmpty(request.Cursor))
        {
            var parts = request.Cursor.Split('_');
            if (parts.Length == 2 &&
                DateTime.TryParse(parts[0], out var parsedDate) &&
                Guid.TryParse(parts[1], out var parsedId))
            {
                cursorDate = parsedDate;
                cursorId = parsedId;
            }
        }

        // Build query with admin user navigation
        var query = _transactionRepository.GetQueryable()
            .Include(t => t.AdminUser)
            .Where(t => t.UserId == request.UserId);

        // Apply cursor pagination
        if (cursorDate.HasValue && cursorId.HasValue)
        {
            query = query.Where(t =>
                t.CreatedAt < cursorDate.Value ||
                (t.CreatedAt == cursorDate.Value && t.Id.CompareTo(cursorId.Value) < 0));
        }

        // Order by CreatedAt descending, then by Id for stable ordering
        query = query.OrderByDescending(t => t.CreatedAt)
                     .ThenByDescending(t => t.Id);

        // Fetch one extra item to determine if there's a next page
        var transactions = await query
            .Take(request.PageSize + 1)
            .ToListAsync(cancellationToken);

        var hasNextPage = transactions.Count > request.PageSize;
        var items = transactions.Take(request.PageSize).ToList();

        // Map to DTOs with admin information
        var dtos = items.Select(t => new AdminTransactionHistoryDto
        {
            Id = t.Id,
            Amount = t.Amount,
            Type = t.TransactionType.ToString(),
            Description = t.Description,
            CreatedAt = t.CreatedAt,
            ReferenceId = t.ReferenceId,
            AdminUserId = t.AdminUserId,
            AdminName = t.AdminUser?.FullName
        }).ToList();

        // Generate next cursor if there's a next page
        string? nextCursor = null;
        if (hasNextPage && items.Any())
        {
            var lastItem = items.Last();
            nextCursor = $"{lastItem.CreatedAt:O}_{lastItem.Id}";
        }

        return new PaginatedResult<AdminTransactionHistoryDto>
        {
            Items = dtos,
            PageSize = request.PageSize,
            NextCursor = nextCursor
        };
    }
}
