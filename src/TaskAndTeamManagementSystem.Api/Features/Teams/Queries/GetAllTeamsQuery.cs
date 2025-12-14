using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Features.Teams.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;

namespace TaskAndTeamManagementSystem.Api.Features.Teams;

public class GetAllTeamsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/teams").WithTags("Teams").RequireAuthorization();

        group.MapGet("/", GetAllTeams)
            .WithName("GetAllTeams")
            .RequireAuthorization("AdminOnly");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<List<TeamDto>>> GetAllTeams(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTeamsQuery();
        return await mediator.Send(query, cancellationToken);
    }
}

public record GetAllTeamsQuery() : IRequest<ApiResponse<List<TeamDto>>>;

public class GetAllTeamsQueryHandler : IRequestHandler<GetAllTeamsQuery, ApiResponse<List<TeamDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTeamsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<List<TeamDto>>> Handle(GetAllTeamsQuery request, CancellationToken cancellationToken)
    {
        var teamRepository = _unitOfWork.Repository<Domain.Entities.Team>();
        var teams = await teamRepository.Query()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var teamDtos = teams.Adapt<List<TeamDto>>();

        return ApiResponse<List<TeamDto>>.SuccessResponse(teamDtos);
    }
}

