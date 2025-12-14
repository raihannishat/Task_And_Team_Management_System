using TaskAndTeamManagementSystem.Api.Domain.Entities;

namespace TaskAndTeamManagementSystem.Api.Infrastructure.Services.Contracts;

public interface IJwtTokenService
{
    System.Threading.Tasks.Task<string> GenerateTokenAsync(User user);
}

