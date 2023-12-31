﻿using TaskService.Enum;

namespace TaskService.Dto.Event;

public record EventInfo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int CalendarId { get; set; }
    public EventType EventType { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string CreatorUsername { get; set; }
    public string? MeetingLink { get; set; }
    public string? Description { get; set; }
    public ICollection<string>? AssignedUsernames { get; set; }
}