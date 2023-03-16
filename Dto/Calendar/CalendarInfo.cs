using StasDiplom.Dto.Event;

namespace StasDiplom.Dto.Calendar;

public record CalendarInfo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int? ProjectId { get; set; }
    public string? Username { get; set; }
    public ICollection<EventInfo> Events { get; set; }
}