using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Domain;
using TaskService.Services.Interfaces;

namespace TaskService.Services;

class NotificationDictionaryService : INotificationDictionaryService
{
    private readonly Dictionary<string, List<Notification>> _dictionary;
    public NotificationDictionaryService(Dictionary<string, List<Notification>> dictionary)
    {
        _dictionary = dictionary;
    }

    public void AddToDictionary(string userId, Notification notification)
    {
        if (!_dictionary.ContainsKey(userId))
        {
            _dictionary.Add(userId, new List<Notification> {notification});
        }
        else
        {
            if (!_dictionary[userId].Contains(notification))
            {
                _dictionary[userId].Add(notification);
            }
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

    public bool IsContain(string userId, Notification notification)
    {
        if (_dictionary.ContainsKey(userId))
        {
            if (_dictionary[userId].Contains(notification))
            {
                return true;
            }
        }

        return false;
    }
}