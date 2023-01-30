using StasDiplom.Enum;

namespace StasDiplom;

public class EventUser
{
    public int EventId { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public EventRole EventRole { get; set; }
    
    public Event Event { get; set; }
}