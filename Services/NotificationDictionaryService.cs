using StasDiplom.Domain;
using StasDiplom.Services.Interfaces;

namespace StasDiplom.Services;

class NotificationDictionaryService : INotificationDictionaryService
{
    private readonly Dictionary<string, List<Notification>> _dictionary;
    private readonly ILogger<NotificationDictionaryService> _logger;

    public NotificationDictionaryService(ILogger<NotificationDictionaryService> logger)
    {
        _logger = logger;
        _dictionary = new Dictionary<string, List<Notification>>();
    }

    public void AddToDictionary(string userId, Notification notification)
    {
        if (!_dictionary.ContainsKey(userId))
        {
            _dictionary.Add(userId, new List<Notification> {notification});
        }
        else
        {
            _dictionary[userId].Add(notification);
        }
        
        //_logger.LogInformation(_dictionary[userId].First().Comment?.Text);
    }

    public void RemoveFromDictionary(string userId, List<int> ids)
    {
        if (_dictionary.ContainsKey(userId))
        {
            _dictionary[userId].RemoveAll(x => ids.Contains(x.Id));

            if (_dictionary[userId].Count == 0)
            {
                _dictionary.Remove(userId);
            }
        }
    }

    public bool IsHasUnread(string userId)
    {
        if (!_dictionary.ContainsKey(userId))
        {
            return false;
        }

        return _dictionary[userId].Count > 0;
    }

    public List<Notification> GetAllNotifications(string userId)
    {
       return !_dictionary.ContainsKey(userId) ? new List<Notification>() : _dictionary[userId];
    }
}