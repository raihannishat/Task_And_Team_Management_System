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

public class CreateTaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization();

        group.MapPost("/", CreateTask)
            .WithName("CreateTask")
            .RequireAuthorization("ManagerOrAdmin");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<TaskDto>> CreateTask(
        CreateTaskCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(command, cancellationToken);
    }
}

public record CreateTaskCommand(
    string Title,
    string Description,
    Guid CreatedByUserId,
    Guid TeamId,
    Guid? AssignToUserId,
    DateTime? DueDate
) : IRequest<ApiResponse<TaskDto>>;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;

    public CreateTaskCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async System.Threading.Tasks.Task<ApiResponse<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var teamRepository = _unitOfWork.Repository<Team>();
        var taskRepository = _unitOfWork.Repository<Task>();

        var createdByUser = await _userManager.FindByIdAsync(request.CreatedByUserId.ToString());
        if (createdByUser == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Created by user not found");
        }

        var team = await teamRepository.GetByIdAsync(request.TeamId);
        if (team == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Team not found");
        }

        if (request.AssignToUserId.HasValue)
        {
            var assignToUser = await _userManager.FindByIdAsync(request.AssignToUserId.Value.ToString());
            if (assignToUser == null)
            {
                return ApiResponse<TaskDto>.ErrorResponse("Assigned user not found");
            }
        }

        var task = new Task
        {
            Title = request.Title,
            Description = request.Description,
            Status = TaskStatus.Todo,
            CreatedByUserId = request.CreatedByUserId,
            TeamId = request.TeamId,
            AssignToUserId = request.AssignToUserId,
            DueDate = request.DueDate
        };

        await taskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();

        var taskWithIncludes = await taskRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignToUser)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == task.Id, cancellationToken);

        if (taskWithIncludes == null)
        {
            return ApiResponse<TaskDto>.ErrorResponse("Task not found after creation");
        }

        var taskDto = taskWithIncludes.Adapt<TaskDto>();

        return ApiResponse<TaskDto>.SuccessResponse(taskDto, "Task created successfully");
    }
}

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required");

        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);
    }
}

