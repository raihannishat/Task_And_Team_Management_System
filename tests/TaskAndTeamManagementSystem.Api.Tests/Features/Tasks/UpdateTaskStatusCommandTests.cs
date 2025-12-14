using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Tasks;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;
using DomainTask = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;
using TaskStatus = TaskAndTeamManagementSystem.Api.Domain.Entities.TaskStatus;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Tasks;

public class UpdateTaskStatusCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<DomainTask>> _repositoryMock;
    private readonly UpdateTaskStatusCommandHandler _handler;

    public UpdateTaskStatusCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<DomainTask>>();
        _unitOfWorkMock.Setup(x => x.Repository<DomainTask>()).Returns(_repositoryMock.Object);
        _handler = new UpdateTaskStatusCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidStatusUpdate_ReturnsSuccessResponse()
    {
        var taskId = Guid.NewGuid();
        var task = new DomainTask
        {
            Id = taskId,
            Title = "Test Task",
            Status = TaskStatus.Todo,
            CreatedByUser = new User { FullName = "Creator" },
            Team = new Team { Name = "Team" }
        };

        var tasks = new List<DomainTask> { task }.AsQueryable();
        _repositoryMock.Setup(x => x.Query()).Returns(tasks);
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var command = new UpdateTaskStatusCommand(taskId, TaskStatus.Done, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        task.Status.Should().Be(TaskStatus.Done);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ReturnsErrorResponse()
    {
        var tasks = new List<DomainTask>().AsQueryable();
        _repositoryMock.Setup(x => x.Query()).Returns(tasks);

        var command = new UpdateTaskStatusCommand(Guid.NewGuid(), TaskStatus.Done, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Task not found");
    }
}

