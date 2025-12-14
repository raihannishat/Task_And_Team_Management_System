using FluentAssertions;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Teams;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Teams;

public class UpdateTeamCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Team>> _repositoryMock;
    private readonly UpdateTeamCommandHandler _handler;

    public UpdateTeamCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Team>>();
        _unitOfWorkMock.Setup(x => x.Repository<Team>()).Returns(_repositoryMock.Object);
        _handler = new UpdateTeamCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidTeam_ReturnsSuccessResponse()
    {
        var teamId = Guid.NewGuid();
        var team = new Team
        {
            Id = teamId,
            Name = "Old Name",
            Description = "Old Description"
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(team);
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var command = new UpdateTeamCommand(teamId, "New Name", "New Description");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        team.Name.Should().Be("New Name");
        team.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task Handle_TeamNotFound_ReturnsErrorResponse()
    {
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Team?)null);

        var command = new UpdateTeamCommand(Guid.NewGuid(), "New Name", "New Description");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Team not found");
    }
}

