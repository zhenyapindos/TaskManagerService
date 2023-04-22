namespace TaskService.Dto.Users.Event;

public class UserEventInteractionRequest
{
    public string Username { get; set; }
    public int EventId { get; set; }
}