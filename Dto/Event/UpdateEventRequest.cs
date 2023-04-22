namespace TaskService.Dto.Event;

public class UpdateEventRequest
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? MeetingLink { get; set; }
    public string? Description { get; set; }
}