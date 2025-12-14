using Mapster;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Teams.Dtos;

namespace TaskAndTeamManagementSystem.Api.Common.Mappings;

public class TeamMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Team, TeamDto>();
    }
}

