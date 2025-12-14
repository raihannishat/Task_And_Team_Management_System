namespace TaskAndTeamManagementSystem.Api.Domain.Entities;

public class Task : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public Guid? AssignToUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid TeamId { get; set; }
    public DateTime? DueDate { get; set; }

    // Navigation properties
    public User? AssignToUser { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public Team Team { get; set; } = null!;
}

public enum TaskStatus
{
    Todo = 1,
    InProgress = 2,
    Done = 3
}

