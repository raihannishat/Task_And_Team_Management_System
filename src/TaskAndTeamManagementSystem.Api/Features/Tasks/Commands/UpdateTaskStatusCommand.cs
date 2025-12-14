using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Common.Extensions;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Tasks.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;
using TaskStatus = TaskAndTeamManagementSystem.Api.Domain.Entities.TaskStatus;

namespace TaskAndTeamManagementSystem.Api.Features.Tasks;

public class UpdateTaskStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization();

        group.MapPut("/{id:guid}/status", UpdateTaskStatus)
            .WithName("UpdateTaskStatus")
            .RequireAuthorization("EmployeeOrAbove");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<TaskDto>> UpdateTaskStatus(
        Guid id,
        UpdateTaskStatusCommand command,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { TaskId = id, User = user };
        return await mediator.Send(updateCommand, cancellationToken);
    }
}

public record UpdateTaskStatusCommand(
    Guid TaskId,
    TaskStatus Status,
    ClaimsPrincipal? User = null
) : IRequest<ApiResponse<TaskDto>>;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<TaskDto>> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
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

        if (request.User != null)
        {
            var userRole = request.User.GetUserRole();
            var userId = request.User.GetUserId();

            if (userRole == UserRole.Employee.ToString() && 
                (task.AssignToUserId != userId || task.AssignToUserId == null))
            {
                return ApiResponse<TaskDto>.ErrorResponse("You can only update status of tasks assigned to you");
            }
        }

        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync();

        var updatedTask = await taskRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignToUser)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (updatedTask == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Task not found after update");
        }

        var taskDto = updatedTask.Adapt<TaskDto>();

        return ApiResponse<TaskDto>.SuccessResponse(taskDto, "Task status updated successfully");
    }
}

public class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
{
    public UpdateTaskStatusCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid task status");
    }
}

