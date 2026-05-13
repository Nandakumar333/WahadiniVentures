using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Course.Commands;

/// <summary>
/// Handler for UpdateCourseCommand
/// Updates existing course with admin authorization check
/// </summary>
public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, CourseDto>
{
    private readonly ICourseService _courseService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCourseCommandHandler> _logger;

    public UpdateCourseCommandHandler(
        ICourseService courseService,
        IMapper mapper,
        ILogger<UpdateCourseCommandHandler> logger)
    {
        _courseService = courseService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CourseDto> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UpdateCourseCommand for CourseId={CourseId}, UpdatedBy={UpdatedBy}",
            request.CourseId,
            request.UpdatedByUserId);

        try
        {
            // Map command to UpdateCourseDto
            var updateCourseDto = _mapper.Map<UpdateCourseDto>(request);

            // Update course via service
            var updatedCourse = await _courseService.UpdateCourseAsync(
                updateCourseDto,
                cancellationToken);

            if (updatedCourse == null)
            {
                _logger.LogError("Failed to update course. CourseId={CourseId}", request.CourseId);
                throw new InvalidOperationException($"Course not found or update failed: {request.CourseId}");
            }

            _logger.LogInformation(
                "Successfully updated course. CourseId={CourseId}",
                updatedCourse.Id);

            return updatedCourse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course. CourseId={CourseId}", request.CourseId);
            throw;
        }
    }
}
