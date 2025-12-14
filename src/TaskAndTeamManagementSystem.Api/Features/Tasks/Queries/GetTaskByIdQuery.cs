using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Features.Tasks.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Features.Tasks;

public class GetTaskByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization();

        group.MapGet("/{id:guid}", GetTaskById)
            .WithName("GetTaskById")
            .RequireAuthorization("EmployeeOrAbove");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<TaskDto>> GetTaskById(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetTaskByIdQuery(id);
        return await mediator.Send(query, cancellationToken);
    }
}

public record GetTaskByIdQuery(Guid Id) : IRequest<ApiResponse<TaskDto>>;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTaskByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var taskRepository = _unitOfWork.Repository<Task>();
        var task = await taskRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignToUser)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Task not found");
        }

        var taskDto = task.Adapt<TaskDto>();

        return ApiResponse<TaskDto>.SuccessResponse(taskDto);
    }
}

