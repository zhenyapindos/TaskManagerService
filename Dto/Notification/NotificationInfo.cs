using StasDiplom.Dto.Event;
using StasDiplom.Dto.Project;
using StasDiplom.Dto.Task;
using StasDiplom.Enum;

namespace StasDiplom.Dto.Notification;

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