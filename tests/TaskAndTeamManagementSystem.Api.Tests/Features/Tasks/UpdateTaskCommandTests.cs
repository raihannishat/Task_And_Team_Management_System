using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Tasks;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;
using DomainTask = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Tasks;

public class UpdateTaskCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<DomainTask>> _taskRepositoryMock;
    private readonly Mock<IRepository<Team>> _teamRepositoryMock;
    private readonly UpdateTaskCommandHandler _handler;

    public UpdateTaskCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<IRepository<DomainTask>>();
        _teamRepositoryMock = new Mock<IRepository<Team>>();
        _unitOfWorkMock.Setup(x => x.Repository<DomainTask>()).Returns(_taskRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Repository<Team>()).Returns(_teamRepositoryMock.Object);
        _handler = new UpdateTaskCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidTask_ReturnsSuccessResponse()
    {
        var taskId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var task = new DomainTask
        {
            Id = taskId,
            Title = "Old Title",
            Description = "Old Description",
            TeamId = teamId,
            CreatedByUser = new User { FullName = "Creator" },
            Team = new Team { Name = "Team" }
        };

        var team = new Team { Id = teamId, Name = "New Team" };

        var tasks = new List<DomainTask> { task }.AsQueryable();
        _taskRepositoryMock.Setup(x => x.Query()).Returns(tasks);
        
        _teamRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(team);
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var command = new UpdateTaskCommand(taskId, "New Title", "New Description", teamId, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        task.Title.Should().Be("New Title");
        task.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task Handle_TaskNotFound_ReturnsErrorResponse()
    {
        var tasks = new List<DomainTask>().AsQueryable();
        _taskRepositoryMock.Setup(x => x.Query()).Returns(tasks);

        var command = new UpdateTaskCommand(Guid.NewGuid(), "New Title", "New Description", Guid.NewGuid(), null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Task not found");
    }
}

