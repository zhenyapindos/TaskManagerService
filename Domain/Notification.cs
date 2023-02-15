namespace StasDiplom.Domain;

public record Notification
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string UserId { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public int NotificationType { get; set; }
    public bool IsRead { get; set; }
    public User User { get; set; }
    public Task? Task { get; set; }
    public Project Project { get; set; }
}