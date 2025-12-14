using FluentAssertions;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Teams;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Teams;

public class CreateTeamCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Team>> _repositoryMock;
    private readonly CreateTeamCommandHandler _handler;

    public CreateTeamCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Team>>();
        _unitOfWorkMock.Setup(x => x.Repository<Team>()).Returns(_repositoryMock.Object);
        _handler = new CreateTeamCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidTeam_ReturnsSuccessResponse()
    {
        var teamId = Guid.NewGuid();
        var team = new Team
        {
            Id = teamId,
            Name = "Test Team",
            Description = "Test Description"
        };

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Team>()))
            .ReturnsAsync(team);
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var command = new CreateTeamCommand("Test Team", "Test Description");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test Team");
        result.Data.Description.Should().Be("Test Description");
    }
}

