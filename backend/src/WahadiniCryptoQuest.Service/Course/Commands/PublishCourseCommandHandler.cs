using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Course.Commands;

/// <summary>
/// Handler for PublishCourseCommand
/// Validates that course has at least one lesson before publishing
/// </summary>
public class PublishCourseCommandHandler : IRequestHandler<PublishCourseCommand, bool>
{
    private readonly ICourseService _courseService;
    private readonly ILogger<PublishCourseCommandHandler> _logger;

    public PublishCourseCommandHandler(
        ICourseService courseService,
        ILogger<PublishCourseCommandHandler> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    public async Task<bool> Handle(PublishCourseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling PublishCourseCommand for CourseId={CourseId}, PublishedBy={PublishedBy}",
            request.CourseId,
            request.PublishedByUserId);

        try
        {
            // Publish course via service (includes lesson count validation)
            var success = await _courseService.PublishCourseAsync(
                request.CourseId,
                cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully published course. CourseId={CourseId}",
                    request.CourseId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to publish course (validation failed or not found). CourseId={CourseId}",
                    request.CourseId);
            }

            return success;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "Cannot publish course (business rule violation). CourseId={CourseId}",
                request.CourseId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing course. CourseId={CourseId}", request.CourseId);
            throw;
        }
    }
}
