using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;

namespace TaskAndTeamManagementSystem.Api.Features.Teams;

public class DeleteTeamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/teams").WithTags("Teams").RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteTeam)
            .WithName("DeleteTeam")
            .RequireAuthorization("AdminOnly");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<bool>> DeleteTeam(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTeamCommand(id);
        return await mediator.Send(command, cancellationToken);
    }
}

public record DeleteTeamCommand(Guid Id) : IRequest<ApiResponse<bool>>;

public class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTeamCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<bool>> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var teamRepository = _unitOfWork.Repository<Domain.Entities.Team>();
        var team = await teamRepository.Query()
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (team == null)
        {
            return ApiResponse<bool>.ErrorResponse("Team not found");
        }

        if (team.Tasks.Any())
        {
            return ApiResponse<bool>.ErrorResponse("Cannot delete team with associated tasks");
        }

        teamRepository.Remove(team);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Team deleted successfully");
    }
}

