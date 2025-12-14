using Microsoft.AspNetCore.Identity;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Infrastructure.Data;

public static class DbSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        if (!await context.Database.CanConnectAsync())
        {
            throw new InvalidOperationException("Cannot connect to the database.");
        }

        var roles = new[] { "Admin", "Manager", "Employee" };
        foreach (var roleName in roles)
        {
            try
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to create role {roleName}: {errors}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error seeding role {roleName}: {ex.Message}", ex);
            }
        }

        // Seed Users
        if (await userManager.FindByEmailAsync("admin@demo.com") == null)
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "admin@demo.com",
                Email = "admin@demo.com",
                FullName = "Admin User",
                Role = UserRole.Admin,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        if (await userManager.FindByEmailAsync("manager@demo.com") == null)
        {
            var managerUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "manager@demo.com",
                Email = "manager@demo.com",
                FullName = "Manager User",
                Role = UserRole.Manager,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(managerUser, "Manager123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }
        }

        if (await userManager.FindByEmailAsync("employee@demo.com") == null)
        {
            var employeeUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "employee@demo.com",
                Email = "employee@demo.com",
                FullName = "Employee User",
                Role = UserRole.Employee,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(employeeUser, "Employee123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(employeeUser, "Employee");
            }
        }
    }
}

