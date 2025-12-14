using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using TaskAndTeamManagementSystem.Api.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace TaskAndTeamManagementSystem.Api.Tests.Infrastructure.Data;

public class DbSeederTests
{
    [Fact]
    public async Task SeedAsync_ShouldCreateAllRoles()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();
        var roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(
            roleStore.Object, null!, null!, null!, null!);

        var roles = new List<IdentityRole<Guid>>();
        roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync((string roleName) => roles.Any(r => r.Name == roleName));
        
        roleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole<Guid>>()))
            .ReturnsAsync((IdentityRole<Guid> role) =>
            {
                roles.Add(role);
                return IdentityResult.Success;
            });

        userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        await DbSeeder.SeedAsync(context, userManager.Object, roleManager.Object);

        roles.Should().HaveCount(3);
        roles.Should().Contain(r => r.Name == "Admin");
        roles.Should().Contain(r => r.Name == "Manager");
        roles.Should().Contain(r => r.Name == "Employee");
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateAdminUser()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();
        var roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(
            roleStore.Object, null!, null!, null!, null!);

        var users = new List<User>();
        var roles = new List<IdentityRole<Guid>> { new IdentityRole<Guid>("Admin") };

        roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        userManager.Setup(x => x.FindByEmailAsync("admin@demo.com"))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.FindByEmailAsync("manager@demo.com"))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.FindByEmailAsync("employee@demo.com"))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync((User user, string password) =>
            {
                users.Add(user);
                return IdentityResult.Success;
            });
        
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        await DbSeeder.SeedAsync(context, userManager.Object, roleManager.Object);

        users.Should().Contain(u => u.Email == "admin@demo.com" && u.Role == UserRole.Admin);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateManagerUser()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();
        var roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(
            roleStore.Object, null!, null!, null!, null!);

        var users = new List<User>();
        var roles = new List<IdentityRole<Guid>> { new IdentityRole<Guid>("Manager") };

        roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        userManager.Setup(x => x.FindByEmailAsync("admin@demo.com"))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.FindByEmailAsync("manager@demo.com"))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.FindByEmailAsync("employee@demo.com"))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync((User user, string password) =>
            {
                users.Add(user);
                return IdentityResult.Success;
            });
        
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        await DbSeeder.SeedAsync(context, userManager.Object, roleManager.Object);

        users.Should().Contain(u => u.Email == "manager@demo.com" && u.Role == UserRole.Manager);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateEmployeeUser()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();
        var roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(
            roleStore.Object, null!, null!, null!, null!);

        var users = new List<User>();
        var roles = new List<IdentityRole<Guid>> { new IdentityRole<Guid>("Employee") };

        roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        userManager.Setup(x => x.FindByEmailAsync("admin@demo.com"))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.FindByEmailAsync("manager@demo.com"))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.FindByEmailAsync("employee@demo.com"))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync((User user, string password) =>
            {
                users.Add(user);
                return IdentityResult.Success;
            });
        
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        await DbSeeder.SeedAsync(context, userManager.Object, roleManager.Object);

        users.Should().Contain(u => u.Email == "employee@demo.com" && u.Role == UserRole.Employee);
    }

    [Fact]
    public async Task SeedAsync_ShouldNotCreateDuplicateUsers()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();
        var roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(
            roleStore.Object, null!, null!, null!, null!);

        var existingAdmin = new User { Email = "admin@demo.com", Role = UserRole.Admin };
        var existingManager = new User { Email = "manager@demo.com", Role = UserRole.Manager };
        var existingEmployee = new User { Email = "employee@demo.com", Role = UserRole.Employee };
        var createCallCount = 0;

        roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        userManager.Setup(x => x.FindByEmailAsync("admin@demo.com"))
            .ReturnsAsync(existingAdmin);
        
        userManager.Setup(x => x.FindByEmailAsync("manager@demo.com"))
            .ReturnsAsync(existingManager);
        
        userManager.Setup(x => x.FindByEmailAsync("employee@demo.com"))
            .ReturnsAsync(existingEmployee);
        
        userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(() =>
            {
                createCallCount++;
                return IdentityResult.Success;
            });

        await DbSeeder.SeedAsync(context, userManager.Object, roleManager.Object);
        await DbSeeder.SeedAsync(context, userManager.Object, roleManager.Object);

        createCallCount.Should().Be(0);
    }

    [Fact]
    public async Task SeedAsync_ShouldSetCorrectUserProperties()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();
        var roleManager = new Mock<RoleManager<IdentityRole<Guid>>>(
            roleStore.Object, null!, null!, null!, null!);

        roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        
        userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        await DbSeeder.SeedAsync(context, userManager.Object, roleManager.Object);

        userManager.Verify(x => x.CreateAsync(
            It.Is<User>(u => 
                u.Email == "admin@demo.com" && 
                u.UserName == "admin@demo.com" &&
                u.FullName == "Admin User" &&
                u.Role == UserRole.Admin &&
                u.EmailConfirmed == true), 
            "Admin123"), Times.Once);
        
        userManager.Verify(x => x.CreateAsync(
            It.Is<User>(u => 
                u.Email == "manager@demo.com" && 
                u.UserName == "manager@demo.com" &&
                u.FullName == "Manager User" &&
                u.Role == UserRole.Manager &&
                u.EmailConfirmed == true), 
            "Manager123"), Times.Once);
        
        userManager.Verify(x => x.CreateAsync(
            It.Is<User>(u => 
                u.Email == "employee@demo.com" && 
                u.UserName == "employee@demo.com" &&
                u.FullName == "Employee User" &&
                u.Role == UserRole.Employee &&
                u.EmailConfirmed == true), 
            "Employee123"), Times.Once);
    }
}

