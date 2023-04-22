using TaskService.Enum;

namespace TaskService.Domain;

public record EventUser
{
    public int EventId { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public EventType EventType { get; set; }
    
    public Event Event { get; set; }
}