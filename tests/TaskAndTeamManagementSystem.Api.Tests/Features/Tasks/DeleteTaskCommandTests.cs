using FluentAssertions;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Tasks;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;
using DomainTask = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Tasks;

public class DeleteTaskCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<DomainTask>> _repositoryMock;
    private readonly DeleteTaskCommandHandler _handler;

    public DeleteTaskCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<DomainTask>>();
        _unitOfWorkMock.Setup(x => x.Repository<DomainTask>()).Returns(_repositoryMock.Object);
        _handler = new DeleteTaskCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidTask_ReturnsSuccessResponse()
    {
        var taskId = Guid.NewGuid();
        var task = new DomainTask
        {
            Id = taskId,
            Title = "Test Task"
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(task);
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var command = new DeleteTaskCommand(taskId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TaskNotFound_ReturnsErrorResponse()
    {
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((DomainTask?)null);

        var command = new DeleteTaskCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Task not found");
    }
}

