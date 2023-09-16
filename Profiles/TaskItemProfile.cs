using AutoMapper;

namespace SmartTasksAPI.Profiles
{
    /// <summary>
    /// Configuration class for AutoMapper
    /// </summary>
    public class TaskItemProfile : Profile
    {
        public TaskItemProfile()
        {
            CreateMap<Entities.TaskItem, Models.TaskItemDto>();
            CreateMap<Models.TaskItemForCreationDto, Entities.TaskItem>();
            CreateMap<Models.TaskItemForUpdateDto, Entities.TaskItem>();
            CreateMap<Entities.TaskItem, Models.TaskItemForUpdateDto>();
        }
    }
}
