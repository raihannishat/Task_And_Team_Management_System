using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Teams;
using TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;
using Task = System.Threading.Tasks.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Teams;

public class GetAllTeamsQueryTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Team>> _repositoryMock;
    private readonly GetAllTeamsQueryHandler _handler;

    public GetAllTeamsQueryTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Team>>();
        _unitOfWorkMock.Setup(x => x.Repository<Team>()).Returns(_repositoryMock.Object);
        _handler = new GetAllTeamsQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllTeams()
    {
        var teams = new List<Team>
        {
            new Team { Id = Guid.NewGuid(), Name = "Team 1", Description = "Description 1" },
            new Team { Id = Guid.NewGuid(), Name = "Team 2", Description = "Description 2" }
        }.AsQueryable();

        _repositoryMock.Setup(x => x.Query()).Returns(teams);

        var query = new GetAllTeamsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().Be(2);
    }
}

