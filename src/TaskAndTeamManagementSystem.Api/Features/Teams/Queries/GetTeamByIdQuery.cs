using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Features.Teams.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;

namespace TaskAndTeamManagementSystem.Api.Features.Teams;

public class GetTeamByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/teams").WithTags("Teams").RequireAuthorization();

        group.MapGet("/{id:guid}", GetTeamById)
            .WithName("GetTeamById")
            .RequireAuthorization("AdminOnly");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<TeamDto>> GetTeamById(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetTeamByIdQuery(id);
        return await mediator.Send(query, cancellationToken);
    }
}

public record GetTeamByIdQuery(Guid Id) : IRequest<ApiResponse<TeamDto>>;

public class GetTeamByIdQueryHandler : IRequestHandler<GetTeamByIdQuery, ApiResponse<TeamDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeamByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<TeamDto>> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var teamRepository = _unitOfWork.Repository<Domain.Entities.Team>();
        var team = await teamRepository.Query()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (team == null)
        {
            return ApiResponse<TeamDto>.ErrorResponse("Team not found");
        }

        var teamDto = team.Adapt<TeamDto>();

        return ApiResponse<TeamDto>.SuccessResponse(teamDto);
    }
}

