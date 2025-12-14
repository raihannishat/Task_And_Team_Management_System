using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Features.Users.Dtos;
using TaskAndTeamManagementSystem.Api.Infrastructure.Data;

namespace TaskAndTeamManagementSystem.Api.Features.Users;

public class GetAllUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/", GetAllUsers)
            .WithName("GetAllUsers")
            .RequireAuthorization("AdminOnly");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<List<UserDto>>> GetAllUsers(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllUsersQuery();
        return await mediator.Send(query, cancellationToken);
    }
}

public record GetAllUsersQuery() : IRequest<ApiResponse<List<UserDto>>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ApiResponse<List<UserDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetAllUsersQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<ApiResponse<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);

        var userDtos = users.Adapt<List<UserDto>>();

        return ApiResponse<List<UserDto>>.SuccessResponse(userDtos);
    }
}

