using StasDiplom.Domain;

namespace StasDiplom.Services.Interfaces;

public interface INotificationDictionaryService
{
    public void AddToDictionary(string userId, Notification notification);
    public void RemoveFromDictionary(string userId, List<int> ids);
    public bool IsHasUnread(string userId);
    public List<Notification> GetAllNotifications(string userId);
}