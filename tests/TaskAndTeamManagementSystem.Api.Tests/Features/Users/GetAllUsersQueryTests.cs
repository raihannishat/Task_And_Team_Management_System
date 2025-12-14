using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Users;
using TaskAndTeamManagementSystem.Api.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Users;

public class GetAllUsersQueryTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _contextMock = new Mock<ApplicationDbContext>(options);
        _handler = new GetAllUsersQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllUsers()
    {
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FullName = "User 1", Email = "user1@example.com", Role = UserRole.Admin },
            new User { Id = Guid.NewGuid(), FullName = "User 2", Email = "user2@example.com", Role = UserRole.Manager }
        }.AsQueryable();

        var usersDbSet = new Mock<DbSet<User>>();
        usersDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        usersDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        usersDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        usersDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        usersDbSet.As<IAsyncEnumerable<User>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<User>(users.GetEnumerator()));

        _contextMock.Setup(x => x.Users).Returns(usersDbSet.Object);

        var query = new GetAllUsersQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().Be(2);
    }
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _enumerator;

    public TestAsyncEnumerator(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator;
    }

    public T Current => _enumerator.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_enumerator.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _enumerator.Dispose();
        return ValueTask.CompletedTask;
    }
}

