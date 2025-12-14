namespace TaskAndTeamManagementSystem.Api.Features.Teams.Dtos;

public record TeamDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt
);

