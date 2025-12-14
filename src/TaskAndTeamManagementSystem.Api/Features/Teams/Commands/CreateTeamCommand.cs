using FluentValidation;
using Mapster;
using MediatR;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Teams.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;

namespace TaskAndTeamManagementSystem.Api.Features.Teams;

public class CreateTeamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/teams").WithTags("Teams").RequireAuthorization();

        group.MapPost("/", CreateTeam)
            .WithName("CreateTeam")
            .RequireAuthorization("AdminOnly");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<TeamDto>> CreateTeam(
        CreateTeamCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(command, cancellationToken);
    }
}

public record CreateTeamCommand(
    string Name,
    string Description
) : IRequest<ApiResponse<TeamDto>>;

public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, ApiResponse<TeamDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTeamCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<TeamDto>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var teamRepository = _unitOfWork.Repository<Team>();
        var team = new Team
        {
            Name = request.Name,
            Description = request.Description
        };

        await teamRepository.AddAsync(team);
        await _unitOfWork.SaveChangesAsync();

        var teamDto = team.Adapt<TeamDto>();

        return ApiResponse<TeamDto>.SuccessResponse(teamDto, "Team created successfully");
    }
}

public class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required")
            .MaximumLength(200).WithMessage("Team name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
    }
}

