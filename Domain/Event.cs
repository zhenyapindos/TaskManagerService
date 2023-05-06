using TaskService.Enum;

namespace TaskService.Domain;

public record Event
{
    public int Id { get; set; }
    public int CalendarId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? MeetingLink { get; set; }
    public EventType EventType { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    //public double DurationHours { get; set; }
    public string CreatorId { get; set; }
    public ICollection<User> Users { get; set; }
    public ICollection<EventUser> EventUsers { get; set; }
    public Calendar Calendar { get; set; }
    public Task? Task { get; set; }
    public int? TaskId { get; set; }
}