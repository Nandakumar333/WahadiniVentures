using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for Course entity
/// </summary>
public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets course with all lessons eagerly loaded
    /// </summary>
    public async Task<Course?> GetWithLessonsAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.Lessons.Where(l => l.IsActive).OrderBy(l => l.OrderIndex))
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
    }

    /// <summary>
    /// Gets published courses by category with pagination
    /// </summary>
    public async Task<PagedResult<Course>> GetByCategoryAsync(
        Guid categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(c => c.CategoryId == categoryId && c.IsPublished && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Course>(items, totalItems, page, pageSize);
    }

    /// <summary>
    /// Search courses by title, description with filters
    /// </summary>
    public async Task<PagedResult<Course>> SearchCoursesAsync(
        string searchTerm,
        DifficultyLevel? difficulty,
        bool? isPremium,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(c => c.IsPublished && !c.IsDeleted);

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(lowerSearchTerm) ||
                c.Description.ToLower().Contains(lowerSearchTerm));
        }

        // Apply difficulty filter
        if (difficulty.HasValue)
        {
            query = query.Where(c => c.DifficultyLevel == difficulty.Value);
        }

        // Apply premium filter
        if (isPremium.HasValue)
        {
            query = query.Where(c => c.IsPremium == isPremium.Value);
        }

        query = query.OrderByDescending(c => c.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Course>(items, totalItems, page, pageSize);
    }

    /// <summary>
    /// Increments the view count for a course
    /// </summary>
    public async Task IncrementViewCountAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"Courses\" SET \"ViewCount\" = \"ViewCount\" + 1 WHERE \"Id\" = {courseId}",
            cancellationToken);
    }

    /// <summary>
    /// Gets courses enrolled by a specific user with progress
    /// </summary>
    public async Task<IEnumerable<Course>> GetEnrolledCoursesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserCourseEnrollment>()
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons.Where(l => l.IsActive))
            .Select(e => e.Course)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all courses for admin panel (including unpublished)
    /// </summary>
    public async Task<PagedResult<Course>> GetAllCoursesForAdminAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.CreatedByUser)
            .OrderByDescending(c => c.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Course>(items, totalItems, page, pageSize);
    }

    /// <summary>
    /// Checks if user is enrolled in a course
    /// </summary>
    public async Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserCourseEnrollment>()
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId, cancellationToken);
    }
}
