using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Tasks.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;
using TaskStatus = TaskAndTeamManagementSystem.Api.Domain.Entities.TaskStatus;

namespace TaskAndTeamManagementSystem.Api.Features.Tasks;

public class AssignTaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization();

        group.MapPut("/{id:guid}/assign", AssignTask)
            .WithName("AssignTask")
            .RequireAuthorization("ManagerOrAdmin");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<TaskDto>> AssignTask(
        Guid id,
        AssignTaskCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var assignCommand = command with { TaskId = id };
        return await mediator.Send(assignCommand, cancellationToken);
    }
}

public record AssignTaskCommand(
    Guid TaskId,
    Guid AssignToUserId
) : IRequest<ApiResponse<TaskDto>>;

public class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;

    public AssignTaskCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async System.Threading.Tasks.Task<ApiResponse<TaskDto>> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var taskRepository = _unitOfWork.Repository<Task>();

        var task = await taskRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignToUser)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Task not found");
        }

        var assignToUser = await _userManager.FindByIdAsync(request.AssignToUserId.ToString());
        if (assignToUser == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("User to assign not found");
        }

        task.AssignToUserId = request.AssignToUserId;
        task.UpdatedAt = DateTime.UtcNow;

        if (task.Status == TaskStatus.Todo)
        {
            task.Status = TaskStatus.InProgress;
        }

        taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync();

        var updatedTask = await taskRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignToUser)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (updatedTask == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Task not found after assignment");
        }

        var taskDto = updatedTask.Adapt<TaskDto>();

        return ApiResponse<TaskDto>.SuccessResponse(taskDto, "Task assigned successfully");
    }
}

public class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
{
    public AssignTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.AssignToUserId)
            .NotEmpty().WithMessage("Assign to user ID is required");
    }
}

