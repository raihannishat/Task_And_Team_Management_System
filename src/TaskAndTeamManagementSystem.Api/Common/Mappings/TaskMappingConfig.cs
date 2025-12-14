using Mapster;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Tasks.Dtos;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Common.Mappings;

public class TaskMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Task, TaskDto>()
            .Map(dest => dest.AssignToUserName, src => src.AssignToUser != null ? src.AssignToUser.FullName : null)
            .Map(dest => dest.CreatedByUserName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null)
            .Map(dest => dest.TeamName, src => src.Team != null ? src.Team.Name : string.Empty);
    }
}

