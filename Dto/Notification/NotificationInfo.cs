using TaskService.Dto.Event;
using TaskService.Dto.Project;
using TaskService.Dto.Task;
using TaskService.Enum;

namespace TaskService.Dto.Notification;

public class NotificationInfo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreationTime { get; set; }
    public bool IsRead { get; set; }
    public NotificationType NotificationType { get; set; }
    public ShortProjectInfo ShortProjectInfo { get; set; }
    public ShortTaskInfo ShortTaskInfo { get; set; }
    public ShortEventInfo ShortEventInfo { get; set; }
}