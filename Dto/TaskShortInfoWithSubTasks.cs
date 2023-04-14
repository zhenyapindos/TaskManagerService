namespace TaskService.Dto;
using Task = StasDiplom.Domain.Task;
public class TaskShortInfoWithSubTasks
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus TaskStatus { get; set; }
    public DateTime? Deadline { get; set; }
    public TaskShortInfoWithSubTasks ParentTask { get; set; }
    public TaskShortInfoWithSubTasks PreviousTask { get; set; }
}