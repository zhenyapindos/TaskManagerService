using StasDiplom.Enum;

namespace StasDiplom.Dto.Project;

public class UserProjectInfo
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserProjectRole UserProjectRole { get; set; }
}