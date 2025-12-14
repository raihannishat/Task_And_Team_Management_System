using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Users;
using TaskAndTeamManagementSystem.Api.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Users;

public class DeleteUserCommandTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _contextMock = new Mock<ApplicationDbContext>(options);
        
        _handler = new DeleteUserCommandHandler(_userManagerMock.Object, _contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsSuccessResponse()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Test User"
        };

        var tasksDbSet = new Mock<DbSet<Domain.Entities.Task>>();
        var tasks = new List<Domain.Entities.Task>().AsQueryable();
        
        tasksDbSet.As<IQueryable<Domain.Entities.Task>>().Setup(m => m.Provider).Returns(tasks.Provider);
        tasksDbSet.As<IQueryable<Domain.Entities.Task>>().Setup(m => m.Expression).Returns(tasks.Expression);
        tasksDbSet.As<IQueryable<Domain.Entities.Task>>().Setup(m => m.ElementType).Returns(tasks.ElementType);
        tasksDbSet.As<IQueryable<Domain.Entities.Task>>().Setup(m => m.GetEnumerator()).Returns(tasks.GetEnumerator());
        tasksDbSet.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Task, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        
        _contextMock.Setup(x => x.Set<Domain.Entities.Task>())
            .Returns(tasksDbSet.Object);
        
        _userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var command = new DeleteUserCommand(userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsErrorResponse()
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new DeleteUserCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_UserWithTasks_ReturnsErrorResponse()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Test User"
        };

        var tasksDbSet = new Mock<DbSet<Domain.Entities.Task>>();
        var tasks = new List<Domain.Entities.Task>().AsQueryable();
        
        tasksDbSet.As<IQueryable<Domain.Entities.Task>>().Setup(m => m.Provider).Returns(tasks.Provider);
        tasksDbSet.As<IQueryable<Domain.Entities.Task>>().Setup(m => m.Expression).Returns(tasks.Expression);
        tasksDbSet.As<IQueryable<Domain.Entities.Task>>().Setup(m => m.ElementType).Returns(tasks.ElementType);
        tasksDbSet.As<IQueryable<Domain.Entities.Task>>().Setup(m => m.GetEnumerator()).Returns(tasks.GetEnumerator());
        tasksDbSet.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Task, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        
        _contextMock.Setup(x => x.Set<Domain.Entities.Task>())
            .Returns(tasksDbSet.Object);

        var command = new DeleteUserCommand(userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete user with associated tasks");
    }
}

