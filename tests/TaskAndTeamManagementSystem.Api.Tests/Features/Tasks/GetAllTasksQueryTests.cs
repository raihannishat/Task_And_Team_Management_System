using FluentAssertions;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Tasks;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;
using DomainTask = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;
using TaskStatus = TaskAndTeamManagementSystem.Api.Domain.Entities.TaskStatus;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Tasks;

public class GetAllTasksQueryTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<DomainTask>> _repositoryMock;
    private readonly GetAllTasksQueryHandler _handler;

    public GetAllTasksQueryTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<DomainTask>>();
        _unitOfWorkMock.Setup(x => x.Repository<DomainTask>()).Returns(_repositoryMock.Object);
        _handler = new GetAllTasksQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllTasks()
    {
        var tasks = new List<DomainTask>
        {
            new DomainTask
            {
                Id = Guid.NewGuid(),
                Title = "Task 1",
                Status = TaskStatus.Todo,
                CreatedByUser = new User { FullName = "User 1" },
                Team = new Team { Name = "Team 1" }
            },
            new DomainTask
            {
                Id = Guid.NewGuid(),
                Title = "Task 2",
                Status = TaskStatus.InProgress,
                CreatedByUser = new User { FullName = "User 2" },
                Team = new Team { Name = "Team 2" }
            }
        }.AsQueryable();

        _repositoryMock.Setup(x => x.Query()).Returns(tasks);

        var query = new GetAllTasksQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsFilteredTasks()
    {
        var tasks = new List<DomainTask>
        {
            new DomainTask
            {
                Id = Guid.NewGuid(),
                Title = "Task 1",
                Status = TaskStatus.Todo,
                CreatedByUser = new User { FullName = "User 1" },
                Team = new Team { Name = "Team 1" }
            },
            new DomainTask
            {
                Id = Guid.NewGuid(),
                Title = "Task 2",
                Status = TaskStatus.InProgress,
                CreatedByUser = new User { FullName = "User 2" },
                Team = new Team { Name = "Team 2" }
            }
        }.AsQueryable();

        _repositoryMock.Setup(x => x.Query()).Returns(tasks);

        var query = new GetAllTasksQuery(Status: TaskStatus.Todo);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
}

