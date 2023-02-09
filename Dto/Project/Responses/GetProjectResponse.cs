using StasDiplom.Domain;
using StasDiplom.Enum;
using Task = StasDiplom.Domain.Task;

namespace StasDiplom.Dto.Project.Responses;

public class GetProjectResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public UserProjectRole UserProjectRole { get; set; }
    public List<User> UserList { get; set; }
    public ICollection<Task> TaskList { get; set; }
}