using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

public class UserTaskSubmissionRepository : Repository<UserTaskSubmission>, IUserTaskSubmissionRepository
{
    public UserTaskSubmissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<UserTaskSubmission>> GetByStatusAsync(
        SubmissionStatus status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(s => s.Task)
            .Include(s => s.User)
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.SubmittedAt);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserTaskSubmission>(items, totalItems, page, pageSize);
    }

    public async Task<PagedResult<UserTaskSubmission>> GetPendingReviewQueueAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(s => s.Task)
            .ThenInclude(t => t.Lesson)
            .Include(s => s.User)
            .Where(s => s.Status == SubmissionStatus.Pending)
            .OrderBy(s => s.SubmittedAt);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserTaskSubmission>(items, totalItems, page, pageSize);
    }

    public async Task<IEnumerable<UserTaskSubmission>> GetUserSubmissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .Include(s => s.Task)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserTaskSubmission?> GetUserTaskSubmissionAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.TaskId == taskId && !s.IsDeleted)
            .Include(s => s.Task)
            .OrderByDescending(s => s.SubmittedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
