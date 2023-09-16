using AutoMapper;

namespace SmartTasksAPI.Profiles
{
    /// <summary>
    /// Configuration class for AutoMapper
    /// </summary>
    public class ChecklistProfile : Profile
    {
        public ChecklistProfile()
        {
            CreateMap<Entities.Checklist, Models.ChecklistDto>();
            CreateMap<Models.ChecklistForCreationDto, Entities.Checklist>();
            CreateMap<Models.ChecklistForUpdateDto, Entities.Checklist>();
            CreateMap<Entities.Checklist, Models.ChecklistForUpdateDto>();
            CreateMap<Entities.Checklist, Models.ChecklistWithTasksDto>();
        }
    }
}
