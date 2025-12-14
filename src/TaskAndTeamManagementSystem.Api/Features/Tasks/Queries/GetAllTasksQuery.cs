using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Features.Tasks.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;
using TaskStatus = TaskAndTeamManagementSystem.Api.Domain.Entities.TaskStatus;

namespace TaskAndTeamManagementSystem.Api.Features.Tasks;

public class GetAllTasksEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization();

        group.MapGet("/", GetAllTasks)
            .WithName("GetAllTasks")
            .RequireAuthorization("EmployeeOrAbove");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<PagedResult<TaskDto>>> GetAllTasks(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        TaskStatus? status = null,
        Guid? assignToUserId = null,
        Guid? teamId = null,
        DateTime? dueDateFrom = null,
        DateTime? dueDateTo = null,
        string? sortBy = "CreatedAt",
        string? sortOrder = "desc",
        IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllTasksQuery(
            pageNumber,
            pageSize,
            searchTerm,
            status,
            assignToUserId,
            teamId,
            dueDateFrom,
            dueDateTo,
            sortBy,
            sortOrder);
        return await mediator.Send(query, cancellationToken);
    }
}

public record GetAllTasksQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    TaskStatus? Status = null,
    Guid? AssignToUserId = null,
    Guid? TeamId = null,
    DateTime? DueDateFrom = null,
    DateTime? DueDateTo = null,
    string? SortBy = "CreatedAt",
    string? SortOrder = "desc"
) : IRequest<ApiResponse<PagedResult<TaskDto>>>;

public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, ApiResponse<PagedResult<TaskDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTasksQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<ApiResponse<PagedResult<TaskDto>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        var taskRepository = _unitOfWork.Repository<Task>();
        IQueryable<Task> query = taskRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignToUser)
            .Include(t => t.Team);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(t => 
                t.Title.Contains(request.SearchTerm) || 
                t.Description.Contains(request.SearchTerm));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        if (request.AssignToUserId.HasValue)
        {
            query = query.Where(t => t.AssignToUserId == request.AssignToUserId.Value);
        }

        if (request.TeamId.HasValue)
        {
            query = query.Where(t => t.TeamId == request.TeamId.Value);
        }

        if (request.DueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate.HasValue && t.DueDate >= request.DueDateFrom.Value);
        }

        if (request.DueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate.HasValue && t.DueDate <= request.DueDateTo.Value);
        }

        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var taskDtos = tasks.Adapt<List<TaskDto>>();

        var result = new PagedResult<TaskDto>
        {
            Items = taskDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResult<TaskDto>>.SuccessResponse(result);
    }

    private static IQueryable<Task> ApplySorting(
        IQueryable<Task> query, 
        string? sortBy, 
        string? sortOrder)
    {
        sortBy = sortBy?.ToLower() ?? "createdat";
        sortOrder = sortOrder?.ToLower() ?? "desc";

        Expression<Func<Task, object>> keySelector = sortBy switch
        {
            "title" => t => t.Title,
            "status" => t => t.Status,
            "duedate" => t => t.DueDate ?? DateTime.MaxValue,
            "createdat" => t => t.CreatedAt,
            _ => t => t.CreatedAt
        };

        return sortOrder == "asc" 
            ? query.OrderBy(keySelector) 
            : query.OrderByDescending(keySelector);
    }
}

