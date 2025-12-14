using MediatR;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Features.Tasks;

public class DeleteTaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteTask)
            .WithName("DeleteTask")
            .RequireAuthorization("ManagerOrAdmin");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<bool>> DeleteTask(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTaskCommand(id);
        return await mediator.Send(command, cancellationToken);
    }
}

public record DeleteTaskCommand(Guid Id) : IRequest<ApiResponse<bool>>;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var taskRepository = _unitOfWork.Repository<Task>();
        var task = await taskRepository.GetByIdAsync(request.Id);

        if (task == null)
        {
            return ApiResponse<bool>.ErrorResponse("Task not found");
        }

        taskRepository.Remove(task);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Task deleted successfully");
    }
}

