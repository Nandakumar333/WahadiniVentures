using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Course.Commands;

/// <summary>
/// Handler for EnrollInCourseCommand
/// Handles duplicate enrollment checks and premium course validation
/// </summary>
public class EnrollInCourseCommandHandler : IRequestHandler<EnrollInCourseCommand, bool>
{
    private readonly ICourseService _courseService;
    private readonly ILogger<EnrollInCourseCommandHandler> _logger;

    public EnrollInCourseCommandHandler(
        ICourseService courseService,
        ILogger<EnrollInCourseCommandHandler> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    public async Task<bool> Handle(EnrollInCourseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling EnrollInCourseCommand for UserId={UserId}, CourseId={CourseId}",
            request.UserId,
            request.CourseId);

        try
        {
            // The CourseService.EnrollUserAsync already handles:
            // - Duplicate enrollment check
            // - Course existence and published status validation
            // - Premium course access (will be added in T071)
            var success = await _courseService.EnrollUserAsync(
                request.CourseId,
                request.UserId,
                cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully enrolled user. UserId={UserId}, CourseId={CourseId}",
                    request.UserId,
                    request.CourseId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to enroll user (possibly already enrolled). UserId={UserId}, CourseId={CourseId}",
                    request.UserId,
                    request.CourseId);
            }

            return success;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "Invalid operation during enrollment. UserId={UserId}, CourseId={CourseId}",
                request.UserId,
                request.CourseId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error enrolling user. UserId={UserId}, CourseId={CourseId}",
                request.UserId,
                request.CourseId);
            throw;
        }
    }
}
