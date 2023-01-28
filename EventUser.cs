namespace StasDiplom;

public class EventUser
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Guid UserId { get; set; }
    public int EventRole { get; set; }
}