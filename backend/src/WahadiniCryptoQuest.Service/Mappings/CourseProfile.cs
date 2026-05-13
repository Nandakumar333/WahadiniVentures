using AutoMapper;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.DTOs.Course;
using CourseEntity = WahadiniCryptoQuest.Core.Entities.Course;
using LessonEntity = WahadiniCryptoQuest.Core.Entities.Lesson;

namespace WahadiniCryptoQuest.Service.Mappings;

/// <summary>
/// AutoMapper profile for Course and Lesson entity mappings
/// </summary>
public class CourseProfile : Profile
{
    public CourseProfile()
    {
        // Course entity to CourseDto
        CreateMap<CourseEntity, CourseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

        // Course entity to CourseDetailDto
        CreateMap<CourseEntity, CourseDetailDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons.OrderBy(l => l.OrderIndex)))
            .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore()) // Will be set in service layer
            .ForMember(dest => dest.UserProgress, opt => opt.Ignore()); // Will be calculated in service layer

        // CreateCourseDto to Course entity
        CreateMap<CreateCourseDto, CourseEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Lessons, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        // CreateCourseCommand to CreateCourseDto
        CreateMap<WahadiniCryptoQuest.Service.Course.Commands.CreateCourseCommand, CreateCourseDto>()
            .ForMember(dest => dest.EstimatedDuration, opt => opt.MapFrom(src => src.EstimatedDurationMinutes));

        // UpdateCourseDto to Course entity
        CreateMap<UpdateCourseDto, CourseEntity>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Lessons, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

        // UpdateCourseCommand to UpdateCourseDto
        CreateMap<WahadiniCryptoQuest.Service.Course.Commands.UpdateCourseCommand, UpdateCourseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.EstimatedDuration, opt => opt.MapFrom(src => src.EstimatedDurationMinutes))
            .ForMember(dest => dest.IsPublished, opt => opt.Ignore());

        // Lesson entity to LessonDto
        CreateMap<LessonEntity, LessonDto>()
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks.OrderBy(t => t.OrderIndex)));

        // CreateLessonDto to Lesson entity
        CreateMap<CreateLessonDto, LessonEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Course, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.UserProgress, opt => opt.Ignore());

        // UpdateLessonDto to Lesson entity
        CreateMap<UpdateLessonDto, LessonEntity>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.UserProgress, opt => opt.Ignore());

        // UserCourseEnrollment to EnrollmentDto
        CreateMap<UserCourseEnrollment, EnrollmentDto>()
            .ForMember(dest => dest.CompletionPercentage, opt => opt.MapFrom(src => src.CompletionPercentage))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted));

        // UserCourseEnrollment with Course to EnrolledCourseDto
        CreateMap<UserCourseEnrollment, EnrolledCourseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Course.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Course.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Course.Description))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Course.Category.Name))
            .ForMember(dest => dest.DifficultyLevel, opt => opt.MapFrom(src => src.Course.DifficultyLevel))
            .ForMember(dest => dest.IsPremium, opt => opt.MapFrom(src => src.Course.IsPremium))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.Course.ThumbnailUrl))
            .ForMember(dest => dest.RewardPoints, opt => opt.MapFrom(src => src.Course.RewardPoints))
            .ForMember(dest => dest.EstimatedDuration, opt => opt.MapFrom(src => src.Course.EstimatedDuration))
            .ForMember(dest => dest.ProgressPercentage, opt => opt.MapFrom(src => src.CompletionPercentage))
            .ForMember(dest => dest.CompletionStatus, opt => opt.MapFrom(src =>
                src.CompletionPercentage >= 100 ? "Completed" :
                src.CompletionPercentage > 0 ? "In Progress" : "Not Started"))
            .ForMember(dest => dest.LastAccessedDate, opt => opt.MapFrom(src => src.LastAccessedAt));
    }
}
