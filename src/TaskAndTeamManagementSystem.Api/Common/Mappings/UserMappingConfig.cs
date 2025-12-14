using Mapster;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Users.Dtos;

namespace TaskAndTeamManagementSystem.Api.Common.Mappings;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Email, src => src.Email ?? string.Empty);
    }
}

