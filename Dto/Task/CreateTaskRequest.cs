namespace StasDiplom.Dto.Task;

public class CreateTaskRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int? ParentTaskId { get; set; }
    public int? PreviousTaskId { get; set; }
    public int ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public double DurationHours { get; set; }
    public string[]? AssignedUsersUsernames { get; set; }
}