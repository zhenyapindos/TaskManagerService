namespace StasDiplom;

public class TaskUser
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public Guid UserId { get; set; }
    public int TaskRole { get; set; }
}