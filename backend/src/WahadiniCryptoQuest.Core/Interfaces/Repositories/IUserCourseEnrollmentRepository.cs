using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.Entities;

namespace WahadiniCryptoQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserCourseEnrollment entity with enrollment operations
/// </summary>
public interface IUserCourseEnrollmentRepository : IRepository<UserCourseEnrollment>
{
    /// <summary>
    /// Enrolls a user in a course
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="courseId">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enrollment record</returns>
   Task<UserCourseEnrollment> EnrollUserAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is enrolled in a specific course
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="courseId">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if enrolled, false otherwise</returns>
   Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
}
