using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Users.Dtos;

namespace TaskAndTeamManagementSystem.Api.Features.Users;

public class UpdateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .RequireAuthorization("AdminOnly");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<UserDto>> UpdateUser(
        Guid id,
        UpdateUserCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { Id = id };
        return await mediator.Send(updateCommand, cancellationToken);
    }
}

public record UpdateUserCommand(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role
) : IRequest<ApiResponse<UserDto>>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public UpdateUserCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async System.Threading.Tasks.Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResponse("User not found");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null && existingUser.Id != request.Id)
        {
            return ApiResponse<UserDto>.ErrorResponse("Email already exists");
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse<UserDto>.ErrorResponse($"Failed to update user: {errors}");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var roleName = request.Role.ToString();
        if (!currentRoles.Contains(roleName))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, roleName);
        }

        var userDto = user.Adapt<UserDto>();

        return ApiResponse<UserDto>.SuccessResponse(userDto, "User updated successfully");
    }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role");
    }
}

