using StasDiplom.Enum;

namespace StasDiplom.Domain;

public class ProjectUser
{
    public int ProjectId { get; set; }
    public string UserId { get; set; }
    public UserProjectRole UserProjectRole { get; set; }
    public User User { get; set; }
    public Project Project { get; set; }
}