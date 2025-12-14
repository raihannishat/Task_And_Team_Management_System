using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using TaskAndTeamManagementSystem.Api.Common.DTOs;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Features.Users;
using Task = System.Threading.Tasks.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Features.Users;

public class UpdateUserCommandTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _handler = new UpdateUserCommandHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsSuccessResponse()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "old@example.com",
            FullName = "Old Name",
            Role = UserRole.Employee
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "Employee" });
        
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var command = new UpdateUserCommand(userId, "New Name", "new@example.com", UserRole.Manager);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FullName.Should().Be("New Name");
        result.Data.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsErrorResponse()
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new UpdateUserCommand(Guid.NewGuid(), "New Name", "new@example.com", UserRole.Manager);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ReturnsErrorResponse()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "old@example.com",
            FullName = "Old Name"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com"
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(existingUser);

        var command = new UpdateUserCommand(userId, "New Name", "existing@example.com", UserRole.Manager);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email already exists");
    }
}

