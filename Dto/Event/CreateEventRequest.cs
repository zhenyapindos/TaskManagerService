using StasDiplom.Enum;
using TaskService.Enum;

namespace StasDiplom.Dto.Event;

public class CreateEventRequest
{
    public string Title { get; set; }
    public int CalendarId { get; set; }
    public EventType EventType { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? MeetingLink { get; set; }
    public string? Description { get; set; }
    public ICollection<string>? Usernames { get; set; }
}