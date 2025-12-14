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

public class GetTaskByIdQueryTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<DomainTask>> _repositoryMock;
    private readonly GetTaskByIdQueryHandler _handler;

    public GetTaskByIdQueryTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<DomainTask>>();
        _unitOfWorkMock.Setup(x => x.Repository<DomainTask>()).Returns(_repositoryMock.Object);
        _handler = new GetTaskByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsTask()
    {
        var taskId = Guid.NewGuid();
        var task = new DomainTask
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.Todo,
            CreatedByUser = new User { FullName = "Creator" },
            Team = new Team { Name = "Team" }
        };

        var tasks = new List<DomainTask> { task }.AsQueryable();
        _repositoryMock.Setup(x => x.Query()).Returns(tasks);

        var query = new GetTaskByIdQuery(taskId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(taskId);
        result.Data.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task Handle_InvalidId_ReturnsErrorResponse()
    {
        var tasks = new List<DomainTask>().AsQueryable();
        _repositoryMock.Setup(x => x.Query()).Returns(tasks);

        var query = new GetTaskByIdQuery(Guid.NewGuid());
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Task not found");
    }
}

