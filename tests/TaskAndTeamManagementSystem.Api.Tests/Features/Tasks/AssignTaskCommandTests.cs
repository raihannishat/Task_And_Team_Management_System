using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Tasks;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;
using DomainTask = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;
using TaskStatus = TaskAndTeamManagementSystem.Api.Domain.Entities.TaskStatus;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Tasks;

public class AssignTaskCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<DomainTask>> _taskRepositoryMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly AssignTaskCommandHandler _handler;

    public AssignTaskCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<IRepository<DomainTask>>();
        _unitOfWorkMock.Setup(x => x.Repository<DomainTask>()).Returns(_taskRepositoryMock.Object);

        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new AssignTaskCommandHandler(_unitOfWorkMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidAssignment_ReturnsSuccessResponse()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new DomainTask
        {
            Id = taskId,
            Title = "Test Task",
            Status = TaskStatus.Todo,
            CreatedByUser = new User { FullName = "Creator" },
            Team = new Team { Name = "Team" }
        };

        var user = new User { Id = userId, FullName = "Assignee" };

        var tasks = new List<DomainTask> { task }.AsQueryable();
        _taskRepositoryMock.Setup(x => x.Query()).Returns(tasks);
        
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var command = new AssignTaskCommand(taskId, userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        task.AssignToUserId.Should().Be(userId);
        task.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ReturnsErrorResponse()
    {
        var tasks = new List<DomainTask>().AsQueryable();
        _taskRepositoryMock.Setup(x => x.Query()).Returns(tasks);

        var command = new AssignTaskCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Task not found");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsErrorResponse()
    {
        var taskId = Guid.NewGuid();
        var task = new DomainTask
        {
            Id = taskId,
            Title = "Test Task",
            CreatedByUser = new User { FullName = "Creator" },
            Team = new Team { Name = "Team" }
        };

        var tasks = new List<DomainTask> { task }.AsQueryable();
        _taskRepositoryMock.Setup(x => x.Query()).Returns(tasks);
        
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new AssignTaskCommand(taskId, Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User to assign not found");
    }
}

