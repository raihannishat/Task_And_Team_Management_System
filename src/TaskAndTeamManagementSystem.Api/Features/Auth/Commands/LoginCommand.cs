using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskAndTeamManagementSystem.Api.Common;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Infrastructure.Services.Contracts;

namespace TaskAndTeamManagementSystem.Api.Features.Auth;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", Login)
            .WithName("Login")
            .AllowAnonymous();
    }

    private static async System.Threading.Tasks.Task<ApiResponse<LoginResponseDto>> Login(
        LoginCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(command, cancellationToken);
    }
}

public record LoginCommand(
    string Email,
    string Password
) : IRequest<ApiResponse<LoginResponseDto>>;

public record LoginResponseDto(
    string Token,
    Guid UserId,
    string Email,
    string FullName,
    string Role
);

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    public async System.Threading.Tasks.Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid email or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid email or password");
        }

        var token = await _jwtTokenService.GenerateTokenAsync(user);

        var response = new LoginResponseDto(
            token,
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            user.Role.ToString()
        );

        return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful");
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

