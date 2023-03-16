using AutoMapper;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto;
using StasDiplom.Dto.Notification;
using StasDiplom.Dto.Project;
using StasDiplom.Dto.Task;
using StasDiplom.Services.Interfaces;
using Task = StasDiplom.Domain.Task;

namespace StasDiplom.Services;

public class NotificationService : INotificationService
{
    private readonly ProjectManagerContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationDictionaryService _dictionary;

    public NotificationService(ProjectManagerContext context, IMapper mapper, INotificationDictionaryService dictionary)
    {
        _context = context;
        _mapper = mapper;
        _dictionary = dictionary;
    }

    public async Task<Notification> ProjectInvitation(Project project, User user)
    {
        var newNotification = Notification.CreateProjectNotification(user, project);
        
        _context.Add(newNotification);
        await _context.SaveChangesAsync();

        _dictionary.AddToDictionary(user.Id, newNotification);
        
        return newNotification;
    }

    public async Task<Notification> ProjectKick(Project project, User user)
    {
        var newNotification = Notification.CreateKickNotification(user, project);
        
        _context.Add(newNotification);
        await _context.SaveChangesAsync();

        _dictionary.AddToDictionary(user.Id, newNotification);
        
        return newNotification;
    }

    public async Task<Notification> TaskAssignUser(Task task, User user)
    {
        var newNotification = Notification.CreateTaskNotification(user, task);
        _context.Add(newNotification);
        
        await _context.SaveChangesAsync();
        
        _dictionary.AddToDictionary(user.Id, newNotification);

        return newNotification;
    }

    public async Task<Notification> UserMention(User user, Comment comment)
    {
        var newNotification = Notification.CreateCommentNotification(user, comment);

        _context.Add(newNotification);
        await _context.SaveChangesAsync();
        
        _dictionary.AddToDictionary(user.Id, newNotification);

        return newNotification;
    }

    public bool IsUnreadNotifications(User user)
    {
        return _dictionary.IsHasUnread(user.Id);
    }

    public async Task<List<NotificationInfo>> GetUnreadNotifications(User user)
    {
        var notificationsInfo = new List<NotificationInfo>();

        foreach (var notification in _dictionary.GetAllNotifications(user.Id))
        {
            var newNotificationInfo = _mapper.Map<NotificationInfo>(notification);
            newNotificationInfo.ShortTaskInfo = _mapper.Map<ShortTaskInfo>(notification.Task);
            newNotificationInfo.ShortProjectInfo = _mapper.Map<ShortProjectInfo>(notification.Project);
            //ToDo: ShortEventInfo = _mapper.Map<ShortProjectInfo>(notification.Event);
            newNotificationInfo.Title = notification.Task == null ? notification.Project.Title : notification.Task.Title;

            notificationsInfo.Add(newNotificationInfo);
        }

        return notificationsInfo;
    }
    
    public void MarkAsRead(User user, List<int> ids)
    {
        var notifications = _context.Notifications.Where(x => ids.Contains(x.Id)).ToList();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        _context.Notifications.UpdateRange(notifications);
        _context.SaveChanges();

        _dictionary.RemoveFromDictionary(user.Id, ids);
    }

    public async Task<List<NotificationInfo>> GetAllNotificationsInfo(User user)
    {
        user.Notifications = _context.Notifications.Where(x => x.User == user).ToList();

        return user.Notifications.Select(notification => _mapper.Map<NotificationInfo>(notification)).ToList();
    }

    public async Task<Notification> EventCreated(User user, Event eventId)
    {
        var newNotification = Notification.CreateEventNotification(user, eventId);

        _context.Add(newNotification);
        await _context.SaveChangesAsync();
        
        _dictionary.AddToDictionary(user.Id, newNotification);

        return newNotification;
    }
}