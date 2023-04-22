using TaskService.Enum;

namespace TaskService.Dto.Project.Responses;

public class UserProjectResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public UserProjectRole UserProjectRole { get; set; }
}