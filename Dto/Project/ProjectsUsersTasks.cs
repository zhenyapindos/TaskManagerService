using TaskService.Dto.Task;

namespace TaskService.Dto.Project;

public class ProjectsUsersTasks
{
    public int ProjectId { get; set; }
    public string Title { get; set; }
    public List<TaskShortInfo>? TaskList { get; set; }
}