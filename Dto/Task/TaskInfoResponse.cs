using TaskService.Dto.Project;
using TaskService.Dto.Users;
using TaskStatus = TaskService.Enum.TaskStatus;

namespace TaskService.Dto.Task;

public class TaskInfoResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskShortInfo ParentTask { get; set; }
    public TaskShortInfo PreviousTask { get; set; }
    public ShortProjectInfo Project { get; set; }
    public DateTime CreationTime { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public double DurationHours { get; set; }
    public DateTime? Deadline { get; set; }
    public IEnumerable<UserShortInfo> AssignedUsers { get; set; }
}