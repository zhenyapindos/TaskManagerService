namespace StasDiplom;

public class Event
{
    public int Id { get; set; }
    public int CalendarId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int? TaskId { get; set; }
    public string? MeetingLink { get; set; }
    public int EventType { get; set; }
    public DateTime StartDate { get; set; }
    public double DurationHours { get; set; }
}