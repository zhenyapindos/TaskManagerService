namespace StasDiplom;

public class Calendar
{
    public int Id { get; set; }
    public int? ProjectId { get; set; }
    public string? UserId { get; set; }
    public string Title { get; set; }
    public User? User { get; set; }
    public ICollection<Event>? Events { get; set; }
    public Project? Project { get; set; }
}