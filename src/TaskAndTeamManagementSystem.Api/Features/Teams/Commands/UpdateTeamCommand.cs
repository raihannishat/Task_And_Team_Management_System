using FluentValidation;
using Mapster;
using MediatR;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Features.Teams.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;

namespace TaskAndTeamManagementSystem.Api.Features.Teams;

public class UpdateTeamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/teams").WithTags("Teams").RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateTeam)
            .WithName("UpdateTeam")
            .RequireAuthorization("AdminOnly");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<TeamDto>> UpdateTeam(
        Guid id,
        UpdateTeamCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { Id = id };
        return await mediator.Send(updateCommand, cancellationToken);
    }
}

public record UpdateTeamCommand(
    Guid Id,
    string Name,
    string Description
) : IRequest<ApiResponse<TeamDto>>;

public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, ApiResponse<TeamDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTeamCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<TeamDto>> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var teamRepository = _unitOfWork.Repository<Domain.Entities.Team>();
        var team = await teamRepository.GetByIdAsync(request.Id);

        if (team == null)
        {
            return ApiResponse<TeamDto>.ErrorResponse("Team not found");
        }

        team.Name = request.Name;
        team.Description = request.Description;
        team.UpdatedAt = DateTime.UtcNow;

        teamRepository.Update(team);
        await _unitOfWork.SaveChangesAsync();

        var teamDto = team.Adapt<TeamDto>();

        return ApiResponse<TeamDto>.SuccessResponse(teamDto, "Team updated successfully");
    }
}

public class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Team ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required")
            .MaximumLength(200).WithMessage("Team name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
    }
}

