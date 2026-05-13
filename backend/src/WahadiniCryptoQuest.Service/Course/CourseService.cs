using AutoMapper;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Common;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Course;

/// <summary>
/// Service implementation for course management operations
/// </summary>
public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IRepository<UserCourseEnrollment> _enrollmentRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository,
        IRepository<UserCourseEnrollment> enrollmentRepository,
        IRepository<Category> _categoryRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        this._categoryRepository = _categoryRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PagedResult<CourseDto>> GetCoursesAsync(
        Guid? categoryId,
        DifficultyLevel? difficultyLevel,
        bool? isPremium,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default,
        bool includeUnpublished = false)
    {
        try
        {
            _logger.LogInformation(
                "Getting courses with filters: CategoryId={CategoryId}, Difficulty={Difficulty}, Premium={Premium}, Search={Search}, Page={Page}, PageSize={PageSize}, IncludeUnpublished={IncludeUnpublished}",
                categoryId, difficultyLevel, isPremium, searchTerm, page, pageSize, includeUnpublished);

            PagedResult<WahadiniCryptoQuest.Core.Entities.Course> pagedCourses;

            // If categoryId is specified, use category-specific search
            if (categoryId.HasValue)
            {
                pagedCourses = await _courseRepository.GetByCategoryAsync(
                    categoryId.Value,
                    page,
                    pageSize,
                    cancellationToken);
            }
            else
            {
                // Use full search with filters
                pagedCourses = await _courseRepository.SearchCoursesAsync(
                    searchTerm ?? string.Empty,
                    difficultyLevel,
                    isPremium,
                    page,
                    pageSize,
                    cancellationToken);
            }

            var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(pagedCourses.Items);

            return new PagedResult<CourseDto>(
                courseDtos,
                pagedCourses.TotalCount,
                pagedCourses.PageNumber,
                pagedCourses.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses with filters");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CourseDetailDto?> GetCourseDetailAsync(
        Guid courseId,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting course details for CourseId={CourseId}, UserId={UserId}", courseId, userId);

            var course = await _courseRepository.GetWithLessonsAsync(courseId, cancellationToken);

            if (course == null)
            {
                _logger.LogWarning("Course not found with Id={CourseId}", courseId);
                return null;
            }

            var courseDetailDto = _mapper.Map<CourseDetailDto>(course);

            // Check enrollment status if user is specified
            if (userId.HasValue)
            {
                var isEnrolled = await _courseRepository.IsUserEnrolledAsync(
                    userId.Value,
                    courseId,
                    cancellationToken);

                // Get enrollment progress if enrolled
                int userProgress = 0;
                if (isEnrolled)
                {
                    var enrollment = await _enrollmentRepository.FirstOrDefaultAsync(
                        e => e.UserId == userId.Value && e.CourseId == courseId);

                    if (enrollment != null)
                    {
                        userProgress = (int)enrollment.CompletionPercentage;
                    }
                }

                // Create new DTO with enrollment info
                courseDetailDto = courseDetailDto with
                {
                    IsEnrolled = isEnrolled,
                    UserProgress = userProgress
                };
            }

            return courseDetailDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course details for CourseId={CourseId}", courseId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> EnrollUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Enrolling UserId={UserId} in CourseId={CourseId}", userId, courseId);

            // Check if already enrolled
            var isEnrolled = await _courseRepository.IsUserEnrolledAsync(userId, courseId, cancellationToken);

            if (isEnrolled)
            {
                _logger.LogWarning("User already enrolled. UserId={UserId}, CourseId={CourseId}", userId, courseId);
                return false;
            }

            // Verify course exists and is published
            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course == null)
            {
                _logger.LogWarning("Course not found with Id={CourseId}", courseId);
                throw new InvalidOperationException($"Course with Id {courseId} not found");
            }

            if (!course.IsPublished)
            {
                _logger.LogWarning("Cannot enroll in unpublished course. CourseId={CourseId}", courseId);
                throw new InvalidOperationException("Cannot enroll in an unpublished course");
            }

            // Premium course validation - check if user has premium access
            if (course.IsPremium)
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found. UserId={UserId}", userId);
                    throw new InvalidOperationException($"User with Id {userId} not found");
                }

                // Check if user has Premium or Admin role
                if (user.Role != UserRoleEnum.Premium && user.Role != UserRoleEnum.Admin)
                {
                    _logger.LogWarning(
                        "Free user attempting to enroll in premium course. UserId={UserId}, CourseId={CourseId}",
                        userId, courseId);
                    throw new WahadiniCryptoQuest.Core.Exceptions.PremiumAccessDeniedException(
                        "You need a premium subscription to access this course. Please upgrade your account.");
                }

                _logger.LogInformation("Premium access validated for UserId={UserId}", userId);
            }

            // Create enrollment
            var enrollment = new UserCourseEnrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                CompletionPercentage = 0,
                IsCompleted = false
            };

            await _enrollmentRepository.AddAsync(enrollment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully enrolled UserId={UserId} in CourseId={CourseId}", userId, courseId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling user. UserId={UserId}, CourseId={CourseId}", userId, courseId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EnrolledCourseDto>> GetUserCoursesAsync(
        Guid userId,
        CompletionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting enrolled courses for UserId={UserId}, Status={Status}", userId, status);

            var enrollments = await _enrollmentRepository.FindAsync(
                e => e.UserId == userId);

            var enrolledCourses = new List<EnrolledCourseDto>();

            foreach (var enrollment in enrollments)
            {
                var course = await _courseRepository.GetWithLessonsAsync(enrollment.CourseId, cancellationToken);

                if (course != null)
                {
                    // Determine completion status based on progress
                    var completionStatus = enrollment.CompletionPercentage switch
                    {
                        100 => CompletionStatus.Completed,
                        > 0 => CompletionStatus.InProgress,
                        _ => CompletionStatus.NotStarted
                    };

                    // Apply filter if specified
                    if (status.HasValue && completionStatus != status.Value)
                    {
                        continue;
                    }

                    var enrolledCourseDto = new EnrolledCourseDto
                    {
                        Id = course.Id,
                        Title = course.Title,
                        Description = course.Description,
                        CategoryName = course.Category?.Name ?? string.Empty,
                        ThumbnailUrl = course.ThumbnailUrl,
                        DifficultyLevel = course.DifficultyLevel,
                        EstimatedDuration = course.EstimatedDuration,
                        IsPremium = course.IsPremium,
                        RewardPoints = course.RewardPoints,
                        ProgressPercentage = enrollment.CompletionPercentage,
                        CompletionStatus = completionStatus,
                        LastAccessedDate = enrollment.LastAccessedAt
                    };

                    enrolledCourses.Add(enrolledCourseDto);
                }
            }

            return enrolledCourses.OrderByDescending(c => c.LastAccessedDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrolled courses for UserId={UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CourseDto> CreateCourseAsync(
        CreateCourseDto createDto,
        Guid createdByUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating course with Title={Title}", createDto.Title);

            // Verify category exists
            var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);

            if (category == null)
            {
                throw new InvalidOperationException($"Category with Id {createDto.CategoryId} not found");
            }

            var course = _mapper.Map<WahadiniCryptoQuest.Core.Entities.Course>(createDto);
            course.CreatedByUserId = createdByUserId;
            course.IsPublished = false; // New courses are unpublished by default
            course.ViewCount = 0;

            await _courseRepository.AddAsync(course);

            _logger.LogInformation("Successfully created course with Id={CourseId}", course.Id);

            return _mapper.Map<CourseDto>(course);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CourseDto?> UpdateCourseAsync(
        UpdateCourseDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating course with Id={CourseId}", updateDto.Id);

            var course = await _courseRepository.GetByIdAsync(updateDto.Id);

            if (course == null)
            {
                _logger.LogWarning("Course not found with Id={CourseId}", updateDto.Id);
                return null;
            }

            // Verify category exists if changed
            if (updateDto.CategoryId != course.CategoryId)
            {
                var category = await _categoryRepository.GetByIdAsync(updateDto.CategoryId);

                if (category == null)
                {
                    throw new InvalidOperationException($"Category with Id {updateDto.CategoryId} not found");
                }
            }

            _mapper.Map(updateDto, course);
            course.UpdatedAt = DateTime.UtcNow;

            await _courseRepository.UpdateAsync(course);

            _logger.LogInformation("Successfully updated course with Id={CourseId}", course.Id);

            return _mapper.Map<CourseDto>(course);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course with Id={CourseId}", updateDto.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting course with Id={CourseId}", courseId);

            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course == null)
            {
                _logger.LogWarning("Course not found with Id={CourseId}", courseId);
                return false;
            }

            await _courseRepository.DeleteAsync(course);

            _logger.LogInformation("Successfully deleted course with Id={CourseId}", courseId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course with Id={CourseId}", courseId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> PublishCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Publishing course with Id={CourseId}", courseId);

            var course = await _courseRepository.GetWithLessonsAsync(courseId, cancellationToken);

            if (course == null)
            {
                _logger.LogWarning("Course not found with Id={CourseId}", courseId);
                return false;
            }

            // Use domain method to publish (validates lessons exist)
            course.Publish();

            await _courseRepository.UpdateAsync(course);

            _logger.LogInformation("Successfully published course with Id={CourseId}", courseId);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot publish course with Id={CourseId}", courseId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing course with Id={CourseId}", courseId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task IncrementViewCountAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Incrementing view count for CourseId={CourseId}", courseId);

            await _courseRepository.IncrementViewCountAsync(courseId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for CourseId={CourseId}", courseId);
            // Don't throw - view count increment failures shouldn't break user experience
        }
    }
}
