namespace TaskService.Domain;

public record Project
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int? CalendarId { get; set; }
    public ICollection<ProjectUser> ProjectUsers { get; set; }
    public ICollection<User> Users { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    public ICollection<Task> Tasks { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public Calendar? Calendar { get; set; }
}