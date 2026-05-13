using AutoMapper;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.DTOs.Task;

namespace WahadiniCryptoQuest.Service.Mappings;

/// <summary>
/// AutoMapper profile for Task entity mappings
/// </summary>
public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        // LearningTask entity to LearningTaskDto
        CreateMap<LearningTask, LearningTaskDto>();

        // UserTaskSubmission entity to TaskSubmissionStatusDto
        CreateMap<UserTaskSubmission, TaskSubmissionStatusDto>()
            .ForMember(dest => dest.SubmissionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.HasSubmitted, opt => opt.MapFrom(src => true));
    }
}
