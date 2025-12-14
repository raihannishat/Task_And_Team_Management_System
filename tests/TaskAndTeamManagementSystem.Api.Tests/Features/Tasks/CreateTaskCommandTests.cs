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

public class CreateTaskCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Team>> _teamRepositoryMock;
    private readonly Mock<IRepository<DomainTask>> _taskRepositoryMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _teamRepositoryMock = new Mock<IRepository<Team>>();
        _taskRepositoryMock = new Mock<IRepository<DomainTask>>();
        _unitOfWorkMock.Setup(x => x.Repository<Team>()).Returns(_teamRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Repository<DomainTask>()).Returns(_taskRepositoryMock.Object);

        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new CreateTaskCommandHandler(_unitOfWorkMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidTask_ReturnsSuccessResponse()
    {
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var user = new User { Id = userId, FullName = "Test User", Email = "test@example.com" };
        var team = new Team { Id = teamId, Name = "Test Team" };
        var task = new DomainTask
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            CreatedByUserId = userId,
            TeamId = teamId,
            Status = TaskStatus.Todo
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        
        _teamRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(team);
        
        _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DomainTask>()))
            .ReturnsAsync(task);
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var tasks = new List<DomainTask> { task }.AsQueryable();
        _taskRepositoryMock.Setup(x => x.Query()).Returns(tasks);

        var command = new CreateTaskCommand("Test Task", "Test Description", userId, teamId, null, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsErrorResponse()
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new CreateTaskCommand("Test Task", "Test Description", Guid.NewGuid(), Guid.NewGuid(), null, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Created by user not found");
    }

    [Fact]
    public async Task Handle_TeamNotFound_ReturnsErrorResponse()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        
        _teamRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Team?)null);

        var command = new CreateTaskCommand("Test Task", "Test Description", userId, Guid.NewGuid(), null, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Team not found");
    }
}

