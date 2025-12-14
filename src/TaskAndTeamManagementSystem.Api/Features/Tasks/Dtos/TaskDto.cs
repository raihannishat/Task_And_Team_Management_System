using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskStatus = TaskAndTeamManagementSystem.Api.Domain.Entities.TaskStatus;

namespace TaskAndTeamManagementSystem.Api.Features.Tasks.Dtos;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    TaskStatus Status,
    Guid? AssignToUserId,
    string? AssignToUserName,
    Guid CreatedByUserId,
    string CreatedByUserName,
    Guid TeamId,
    string TeamName,
    DateTime? DueDate,
    DateTime CreatedAt
);

