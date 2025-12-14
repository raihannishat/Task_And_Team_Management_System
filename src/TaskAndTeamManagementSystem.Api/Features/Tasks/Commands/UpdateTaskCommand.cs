using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Features.Tasks.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Features.Tasks;

public class UpdateTaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateTask)
            .WithName("UpdateTask")
            .RequireAuthorization("ManagerOrAdmin");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<TaskDto>> UpdateTask(
        Guid id,
        UpdateTaskCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { Id = id };
        return await mediator.Send(updateCommand, cancellationToken);
    }
}

public record UpdateTaskCommand(
    Guid Id,
    string Title,
    string Description,
    Guid TeamId,
    DateTime? DueDate
) : IRequest<ApiResponse<TaskDto>>;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskRepository = _unitOfWork.Repository<Task>();
        var teamRepository = _unitOfWork.Repository<Domain.Entities.Team>();

        var task = await taskRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignToUser)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Task not found");
        }

        var team = await teamRepository.GetByIdAsync(request.TeamId);
        if (team == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Team not found");
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.TeamId = request.TeamId;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync();

        var updatedTask = await taskRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignToUser)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (updatedTask == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Task not found after update");
        }

        var taskDto = updatedTask.Adapt<TaskDto>();

        return ApiResponse<TaskDto>.SuccessResponse(taskDto, "Task updated successfully");
    }
}

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);
    }
}

