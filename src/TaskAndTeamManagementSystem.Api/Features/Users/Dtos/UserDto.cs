using TaskAndTeamManagementSystem.Api.Domain.Entities;

namespace TaskAndTeamManagementSystem.Api.Features.Users.Dtos;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    DateTime CreatedAt
);

