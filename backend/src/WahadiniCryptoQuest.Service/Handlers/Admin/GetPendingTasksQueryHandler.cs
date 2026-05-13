using MediatR;
using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.DTOs.Admin;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.Core.Interfaces.Repositories;

namespace WahadiniCryptoQuest.Service.Handlers.Admin;

/// <summary>
/// Handler for retrieving pending task submissions
/// T045: US2 - Task Review Workflow
/// </summary>
public class GetPendingTasksQueryHandler : IRequestHandler<Queries.Admin.GetPendingTasksQuery, List<PendingTaskDto>>
{
    private readonly IUserTaskSubmissionRepository _taskSubmissionRepository;

    public GetPendingTasksQueryHandler(IUserTaskSubmissionRepository taskSubmissionRepository)
    {
        _taskSubmissionRepository = taskSubmissionRepository;
    }

    public async Task<List<PendingTaskDto>> Handle(Queries.Admin.GetPendingTasksQuery request, CancellationToken cancellationToken)
    {
        // Get pending submissions with related data
        var submissions = await _taskSubmissionRepository.FindAsync(
            s => s.Status == SubmissionStatus.Pending);

        // Apply date filters
        if (request.DateFrom.HasValue)
        {
            submissions = submissions.Where(s => s.SubmittedAt >= request.DateFrom.Value).ToList();
        }

        if (request.DateTo.HasValue)
        {
            submissions = submissions.Where(s => s.SubmittedAt <= request.DateTo.Value).ToList();
        }

        // Apply course filter
        if (request.CourseId.HasValue)
        {
            submissions = submissions.Where(s => s.Task.Lesson.CourseId == request.CourseId.Value).ToList();
        }

        // Order by submission date (newest first)
        submissions = submissions.OrderByDescending(s => s.SubmittedAt).ToList();

        // Apply pagination
        var skip = (request.PageNumber - 1) * request.PageSize;
        submissions = submissions.Skip(skip).Take(request.PageSize).ToList();

        // Map to DTOs
        var result = submissions.Select(s => new PendingTaskDto
        {
            SubmissionId = s.Id,
            UserId = s.UserId,
            Username = s.User.Email,
            TaskId = s.TaskId,
            TaskTitle = s.Task.Title,
            CourseName = s.Task.Lesson.Course.Title,
            SubmittedAt = s.SubmittedAt,
            ContentType = "JSONB", // All submissions are stored as JSONB
            SubmissionData = s.SubmissionData,
            PointReward = s.Task.RewardPoints
        }).ToList();

        return result;
    }
}
