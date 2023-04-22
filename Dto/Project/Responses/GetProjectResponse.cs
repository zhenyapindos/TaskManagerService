using TaskService.Dto.Users;
using TaskService.Enum;

namespace TaskService.Dto.Project.Responses;

public record GetProjectResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public UserProjectRole UserProjectRole { get; set; }
    public List<UserShortInfo> UserList { get; set; }
    public ICollection<TaskShortInfoWithSubTasks> TaskList { get; set; }
}