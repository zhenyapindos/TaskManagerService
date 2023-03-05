using StasDiplom.Enum;

namespace StasDiplom.Dto.Project;

public record UserShortInfo
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserProjectRole UserProjectRole { get; set; }
}