using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Teams;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Teams;

public class DeleteTeamCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Team>> _repositoryMock;
    private readonly DeleteTeamCommandHandler _handler;

    public DeleteTeamCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Team>>();
        _unitOfWorkMock.Setup(x => x.Repository<Team>()).Returns(_repositoryMock.Object);
        _handler = new DeleteTeamCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidTeam_ReturnsSuccessResponse()
    {
        var teamId = Guid.NewGuid();
        var team = new Team
        {
            Id = teamId,
            Name = "Test Team",
            Description = "Test Description",
            Tasks = new List<Domain.Entities.Task>()
        };

        var teams = new List<Team> { team }.AsQueryable();
        _repositoryMock.Setup(x => x.Query())
            .Returns(teams);
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var command = new DeleteTeamCommand(teamId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TeamNotFound_ReturnsErrorResponse()
    {
        var teams = new List<Team>().AsQueryable();
        _repositoryMock.Setup(x => x.Query())
            .Returns(teams);

        var command = new DeleteTeamCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Team not found");
    }

    [Fact]
    public async Task Handle_TeamWithTasks_ReturnsErrorResponse()
    {
        var teamId = Guid.NewGuid();
        var team = new Team
        {
            Id = teamId,
            Name = "Test Team",
            Tasks = new List<Domain.Entities.Task> { new Domain.Entities.Task { Id = Guid.NewGuid() } }
        };

        var teams = new List<Team> { team }.AsQueryable();
        _repositoryMock.Setup(x => x.Query())
            .Returns(teams);

        var command = new DeleteTeamCommand(teamId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete team with associated tasks");
    }
}

