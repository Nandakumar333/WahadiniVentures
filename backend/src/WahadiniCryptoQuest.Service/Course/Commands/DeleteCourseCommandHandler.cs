using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Course.Commands;

/// <summary>
/// Handler for DeleteCourseCommand
/// Performs soft delete by setting IsDeleted flag
/// </summary>
public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, bool>
{
    private readonly ICourseService _courseService;
    private readonly ILogger<DeleteCourseCommandHandler> _logger;

    public DeleteCourseCommandHandler(
        ICourseService courseService,
        ILogger<DeleteCourseCommandHandler> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling DeleteCourseCommand for CourseId={CourseId}, DeletedBy={DeletedBy}",
            request.CourseId,
            request.DeletedByUserId);

        try
        {
            // Soft delete via service
            var success = await _courseService.DeleteCourseAsync(
                request.CourseId,
                cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully deleted course. CourseId={CourseId}",
                    request.CourseId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to delete course (possibly not found). CourseId={CourseId}",
                    request.CourseId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course. CourseId={CourseId}", request.CourseId);
            throw;
        }
    }
}
