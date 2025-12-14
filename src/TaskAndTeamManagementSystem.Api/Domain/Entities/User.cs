using Microsoft.AspNetCore.Identity;

namespace TaskAndTeamManagementSystem.Api.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Task> CreatedTasks { get; set; } = new List<Task>();
    public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Employee = 3
}

