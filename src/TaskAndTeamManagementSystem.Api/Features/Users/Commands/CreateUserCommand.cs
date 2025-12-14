using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Users.Dtos;

namespace TaskAndTeamManagementSystem.Api.Features.Users;

public class CreateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .RequireAuthorization("AdminOnly");
    }

    private static async System.Threading.Tasks.Task<ApiResponse<UserDto>> CreateUser(
        CreateUserCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(command, cancellationToken);
    }
}

public record CreateUserCommand(
    string FullName,
    string Email,
    string Password,
    UserRole Role
) : IRequest<ApiResponse<UserDto>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public CreateUserCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async System.Threading.Tasks.Task<ApiResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<UserDto>.ErrorResponse("Email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Role = request.Role,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse<UserDto>.ErrorResponse($"Failed to create user: {errors}");
        }

        var roleName = request.Role.ToString();
        await _userManager.AddToRoleAsync(user, roleName);

        var userDto = user.Adapt<UserDto>();

        return ApiResponse<UserDto>.SuccessResponse(userDto, "User created successfully");
    }
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role");
    }
}

