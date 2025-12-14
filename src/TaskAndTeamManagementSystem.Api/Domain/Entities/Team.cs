namespace TaskAndTeamManagementSystem.Api.Domain.Entities;

public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
    public ICollection<User> Members { get; set; } = new List<User>();
}

