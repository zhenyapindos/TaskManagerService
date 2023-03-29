using StasDiplom.Dto.Task;

namespace StasDiplom.Dto.Project;

public class ProjectsUsersTasks
{
    public int ProjectId { get; set; }
    public string Title { get; set; }
    public List<TaskShortInfo>? TaskList { get; set; }
}