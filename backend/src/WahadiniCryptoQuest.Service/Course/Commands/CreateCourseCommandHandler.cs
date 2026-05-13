using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WahadiniCryptoQuest.Core.DTOs.Course;
using WahadiniCryptoQuest.Core.Interfaces.Services;

namespace WahadiniCryptoQuest.Service.Course.Commands;

/// <summary>
/// Handler for CreateCourseCommand
/// Creates a new course with draft status
/// </summary>
public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly ICourseService _courseService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCourseCommandHandler> _logger;

    public CreateCourseCommandHandler(
        ICourseService courseService,
        IMapper mapper,
        ILogger<CreateCourseCommandHandler> logger)
    {
        _courseService = courseService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CourseDto> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CreateCourseCommand for Title={Title}, CategoryId={CategoryId}, CreatedBy={CreatedBy}",
            request.Title,
            request.CategoryId,
            request.CreatedByUserId);

        try
        {
            // Map command to CreateCourseDto
            var createCourseDto = _mapper.Map<CreateCourseDto>(request);

            // Create course via service
            var createdCourse = await _courseService.CreateCourseAsync(
                createCourseDto,
                request.CreatedByUserId,
                cancellationToken);

            if (createdCourse == null)
            {
                _logger.LogError("Failed to create course. Title={Title}", request.Title);
                throw new InvalidOperationException($"Failed to create course: {request.Title}");
            }

            _logger.LogInformation(
                "Successfully created course. CourseId={CourseId}, Title={Title}",
                createdCourse.Id,
                createdCourse.Title);

            return createdCourse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course. Title={Title}", request.Title);
            throw;
        }
    }
}
