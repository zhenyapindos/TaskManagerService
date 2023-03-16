using StasDiplom.Enum;

namespace StasDiplom.Domain;

public record Notification
{
    public int Id { get; set; }
    public DateTime CreationTime { get; set; }
    public string UserId { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public NotificationType NotificationType { get; set; }
    public bool IsRead { get; set; }
    public User User { get; set; }
    public Task? Task { get; set; }
    public Project? Project { get; set; }
    public Comment? Comment { get; set; }
    public Event? Event { get; set; }
    public int? EventId { get; set; }
    public int? CommentId { get; set; }

    public static Notification CreateTaskNotification(User user, Task task)
    {
        return new Notification()
        {
            CreationTime = DateTime.Now,
            IsRead = false,
            Task = task,
            User = user,
            Project = task.Project,
            NotificationType = NotificationType.TaskAssigment
        };
    }
    
    public static Notification CreateProjectNotification(User user, Project project)
    {
        return new Notification()
        {
            CreationTime = DateTime.Now,
            IsRead = false,
            User = user,
            Project = project,
            NotificationType = NotificationType.ProjectInvitation
        };
    }
    
    public static Notification CreateCommentNotification(User user, Comment comment)
    {
        return new Notification
        {
            CreationTime = DateTime.Now,
            IsRead = false,
            User = user,
            Project = comment.Project,
            Task = comment.Task,
            NotificationType = NotificationType.Mention,
            Comment = comment
        };
    }
    
    public static Notification CreateKickNotification(User user, Project project)
    {
        return new Notification()
        {
            CreationTime = DateTime.Now,
            IsRead = false,
            User = user,
            Project = project,
            NotificationType = NotificationType.KickedProject
        };
    }

    public static Notification CreateEventNotification(User user, Event eventId)
    {
        return new Notification()
        {
            CreationTime = DateTime.Now,
            IsRead = false,
            User = user,
            Event = eventId,
            NotificationType = NotificationType.EventCreated
        };
    }
}