namespace StasDiplom.Dto.Task;

public class TaskShortInfo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus TaskStatus { get; set; }
    public DateTime? Deadline { get; set; }
}