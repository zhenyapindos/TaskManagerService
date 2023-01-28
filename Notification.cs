namespace StasDiplom;

public class Notification
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public Guid UserId { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public int NotificationType { get; set; }
    public bool IsRead { get; set; }
}