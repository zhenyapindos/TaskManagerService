using Microsoft.AspNetCore.Identity;
using StasDiplom.Enum;

namespace StasDiplom;

public class ProjectUser
{
    public int ProjectId { get; set; }
    public string UserId { get; set; }
    public ProjectRole ProjectRole { get; set; }
    public User User { get; set; }
    public Project Project { get; set; }
}