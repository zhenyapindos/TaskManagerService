using StasDiplom.Enum;

namespace StasDiplom.Domain;

public class TaskUser
{
    public int TaskId { get; set; }
    public string UserId { get; set; }
    public TaskRole TaskRole { get; set; }
    public User User { get; set; }

    public Task Task { get; set; }
}