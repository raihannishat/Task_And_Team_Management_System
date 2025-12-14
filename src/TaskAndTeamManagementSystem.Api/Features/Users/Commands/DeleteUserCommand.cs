using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Infrastructure.Data;

namespace TaskAndTeamManagementSystem.Api.Features.Users;

public class DeleteUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteUser)
            .WithName("DeleteUser")
            .RequireAuthorization("AdminOnly");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<bool>> DeleteUser(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(id);
        return await mediator.Send(command, cancellationToken);
    }
}

public record DeleteUserCommand(Guid Id) : IRequest<ApiResponse<bool>>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponse<bool>>
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public DeleteUserCommandHandler(UserManager<User> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async System.Threading.Tasks.Task<ApiResponse<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user == null)
        {
            return ApiResponse<bool>.ErrorResponse("User not found");
        }

        var hasCreatedTasks = await _context.Set<Domain.Entities.Task>()
            .AnyAsync(t => t.CreatedByUserId == request.Id, cancellationToken);
        var hasAssignedTasks = await _context.Set<Domain.Entities.Task>()
            .AnyAsync(t => t.AssignToUserId == request.Id, cancellationToken);

        if (hasCreatedTasks || hasAssignedTasks)
        {
            return ApiResponse<bool>.ErrorResponse("Cannot delete user with associated tasks");
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse<bool>.ErrorResponse($"Failed to delete user: {errors}");
        }

        return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
    }
}

