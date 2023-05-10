using TaskStatus = TaskService.Enum.TaskStatus;

namespace TaskService.Domain;

public record Task
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus TaskStatus { get; set; }
    public DateTime CreationTime { get; set; }
    public double? DurationHours { get; set; }
    public int? ParentTaskId { get; set; }
    public int? PreviousTaskId { get; set; }
    public DateTime? StartDate { get; set; }
    public int ProjectId { get; set; }
    public DateTime? Deadline { get; set; }
    public Project Project { get; set; }
    public ICollection<TaskUser> TaskUsers { get; set; }
    public ICollection<User> Users { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Event> Events { get; set; }
}