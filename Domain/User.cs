using Microsoft.AspNetCore.Identity;

namespace TaskService.Domain;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public ICollection<Project> Projects { get; set; }
    public ICollection<ProjectUser> ProjectUsers { get; set; }
    public ICollection<Task> Tasks { get; set; }
    public ICollection<TaskUser> TaskUsers { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Event> Events { get; set; }
    public ICollection<EventUser> EventUsers { get; set; }
}