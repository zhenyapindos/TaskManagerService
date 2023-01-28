namespace StasDiplom;

public class ProjectUser
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Guid UserId { get; set; }
    public int ProjectRole { get; set; }
}