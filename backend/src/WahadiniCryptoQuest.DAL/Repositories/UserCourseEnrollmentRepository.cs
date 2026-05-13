using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Repositories;

/// <summary>
/// Repository implementation for UserCourseEnrollment entity
/// </summary>
public class UserCourseEnrollmentRepository : Repository<UserCourseEnrollment>, IUserCourseEnrollmentRepository
{
    public UserCourseEnrollmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Enrolls a user in a course
    /// </summary>
    public async Task<UserCourseEnrollment> EnrollUserAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var enrollment = new UserCourseEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow,
            CompletionPercentage = 0,
            IsCompleted = false,
            CompletedAt = null
        };

        await _dbSet.AddAsync(enrollment, cancellationToken);
        return enrollment;
    }

    /// <summary>
    /// Checks if a user is enrolled in a course
    /// </summary>
    public async Task<bool> IsUserEnrolledAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId, cancellationToken);
    }
}
