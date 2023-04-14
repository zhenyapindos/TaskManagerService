using StasDiplom.Domain;
using StasDiplom.Dto.Task;
using StasDiplom.Dto.Users;
using StasDiplom.Enum;
using TaskService.Dto;
using Task = StasDiplom.Domain.Task;

namespace StasDiplom.Dto.Project.Responses;

public record GetProjectResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public UserProjectRole UserProjectRole { get; set; }
    public List<UserShortInfo> UserList { get; set; }
    public ICollection<TaskShortInfoWithSubTasks> TaskList { get; set; }
}