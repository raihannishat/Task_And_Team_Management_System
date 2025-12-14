using FluentAssertions;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Teams;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Teams;

public class GetTeamByIdQueryTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Team>> _repositoryMock;
    private readonly GetTeamByIdQueryHandler _handler;

    public GetTeamByIdQueryTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Team>>();
        _unitOfWorkMock.Setup(x => x.Repository<Team>()).Returns(_repositoryMock.Object);
        _handler = new GetTeamByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsTeam()
    {
        var teamId = Guid.NewGuid();
        var team = new Team
        {
            Id = teamId,
            Name = "Test Team",
            Description = "Test Description"
        };

        var teams = new List<Team> { team }.AsQueryable();
        _repositoryMock.Setup(x => x.Query()).Returns(teams);

        var query = new GetTeamByIdQuery(teamId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(teamId);
        result.Data.Name.Should().Be("Test Team");
    }

    [Fact]
    public async Task Handle_InvalidId_ReturnsErrorResponse()
    {
        var teams = new List<Team>().AsQueryable();
        _repositoryMock.Setup(x => x.Query()).Returns(teams);

        var query = new GetTeamByIdQuery(Guid.NewGuid());
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Team not found");
    }
}

