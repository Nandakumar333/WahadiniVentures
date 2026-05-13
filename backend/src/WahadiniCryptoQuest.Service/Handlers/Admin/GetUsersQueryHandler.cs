using MediatR;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for retrieving paginated user list with filters
/// T062: US3 - User Account Management
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<Queries.Admin.GetUsersQuery, PaginatedUsersDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetUsersQueryHandler(
        IUserRepository userRepository,
        ISubscriptionRepository subscriptionRepository)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<PaginatedUsersDto> Handle(Queries.Admin.GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Build filter expression
        var users = await _userRepository.GetAllAsync();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            users = users.Where(u =>
                u.Email.ToLower().Contains(searchLower) ||
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower)).ToList();
        }

        // Apply role filter
        if (request.Role.HasValue)
        {
            users = users.Where(u => u.Role == request.Role.Value).ToList();
        }

        // Apply active filter
        if (request.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == request.IsActive.Value).ToList();
        }

        // Apply banned filter
        if (request.IsBanned.HasValue)
        {
            users = users.Where(u => u.IsBanned == request.IsBanned.Value).ToList();
        }

        // Apply email confirmed filter
        if (request.EmailConfirmed.HasValue)
        {
            users = users.Where(u => u.EmailConfirmed == request.EmailConfirmed.Value).ToList();
        }

        // Apply subscription filter
        if (request.HasActiveSubscription.HasValue)
        {
            var subscriptions = await _subscriptionRepository.FindAsync(s => s.Status == SubscriptionStatus.Active);
            var userIdsWithSub = subscriptions.Select(s => s.UserId).ToHashSet();

            users = request.HasActiveSubscription.Value
                ? users.Where(u => userIdsWithSub.Contains(u.Id)).ToList()
                : users.Where(u => !userIdsWithSub.Contains(u.Id)).ToList();
        }

        // Get total count before pagination
        var totalCount = users.Count;

        // Apply sorting
        users = request.SortBy.ToLower() switch
        {
            "email" => request.SortDescending
                ? users.OrderByDescending(u => u.Email).ToList()
                : users.OrderBy(u => u.Email).ToList(),
            "lastloginat" => request.SortDescending
                ? users.OrderByDescending(u => u.LastLoginAt ?? DateTime.MinValue).ToList()
                : users.OrderBy(u => u.LastLoginAt ?? DateTime.MinValue).ToList(),
            "points" => request.SortDescending
                ? users.OrderByDescending(u => u.CurrentPoints).ToList()
                : users.OrderBy(u => u.CurrentPoints).ToList(),
            _ => request.SortDescending
                ? users.OrderByDescending(u => u.CreatedAt).ToList()
                : users.OrderBy(u => u.CreatedAt).ToList()
        };

        // Apply pagination
        var skip = (request.PageNumber - 1) * request.PageSize;
        users = users.Skip(skip).Take(request.PageSize).ToList();

        // Map to DTOs
        var userDtos = users.Select(u => new UserSummaryDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role,
            CurrentPoints = u.CurrentPoints,
            EmailConfirmed = u.EmailConfirmed,
            IsActive = u.IsActive,
            IsBanned = u.IsBanned,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt
        }).ToList();

        // Build paginated result
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PaginatedUsersDto
        {
            Users = userDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages
        };
    }
}
