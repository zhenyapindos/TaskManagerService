using StasDiplom.Dto.Event;
using StasDiplom.Dto.Users.Event;
using Task = System.Threading.Tasks.Task;

namespace StasDiplom.Services.Interfaces;

public interface IEventService
{
    public Task<EventInfo> CreateEvent(CreateEventRequest request, string id);
    public Task<EventInfo> UpdateEvent(UpdateEventRequest request, string id);
    public void DeleteEvent(int eventId, string id);

    public Task AssignUser(UserEventInteractionRequest request, string id);
    public Task UnassignUser(UserEventInteractionRequest request, string id);
    public Task PostTaskAsEvent(int taskId, string id);
}