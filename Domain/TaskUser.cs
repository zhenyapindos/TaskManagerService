using StasDiplom.Enum;
using TaskStatus = StasDiplom.Enum.TaskStatus;

namespace StasDiplom.Domain;

public class TaskUser
{
    public int TaskId { get; set; }
    public string UserId { get; set; }
    public TaskStatus TaskStatus { get; set; }
    public User User { get; set; }

    public Task Task { get; set; }
}