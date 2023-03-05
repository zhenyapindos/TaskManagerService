using StasDiplom.Domain;
using StasDiplom.Dto;
using Task = StasDiplom.Domain.Task;

namespace StasDiplom.Services;

public interface INotificationService
{
    public Task<Notification> ProjectInvitation(Project project, User user);
    public Task<Notification> ProjectKick(Project project, User user);
    public Task<Notification> TaskAssignUser(Task task, User user);
    public Task<Notification> UserMention(User user, Comment comment);
    public bool IsUnreadNotifications(User user);
    public Task<List<NotificationInfo>> GetUnreadNotifications(User user);
    public void MarkAsRead(User user, int id);
    public Task<List<NotificationInfo>> GetAllNotificationsInfo(User user);
    //public void AddToDictionary(Dictionary<User, List<Notification>> dictionary, Notification notification);
}