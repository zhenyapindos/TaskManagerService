using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Domain;
using TaskService.Dto.Event;
using TaskService.Dto.Notification;
using TaskService.Dto.Project;
using TaskService.Dto.Task;
using TaskService.Services.Interfaces;
using Task = TaskService.Domain.Task;

namespace TaskService.Services;

public class NotificationService : INotificationService
{
    private readonly ProjectManagerContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationDictionaryService _dictionary;

    public NotificationService(ProjectManagerContext context, IMapper mapper, INotificationDictionaryService dictionary, UserManager<User> userManager)
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
            newNotificationInfo.ShortEventInfo = _mapper.Map<ShortEventInfo>(notification.Event);
            //newNotificationInfo.Title = notification.Task == null ? notification.Project.Title : notification.Task.Title;

            notificationsInfo.Add(newNotificationInfo);
        }

        return notificationsInfo;
    }
    public async Task<List<NotificationInfo>> GetAllNotificationsInfo(User user)
    {
        user.Notifications = _context.Notifications
            .Include(x => x.Project)
            .Include(x => x.Task)
            .Include(x => x.Event)
            .Where(x => x.User == user).ToList();

        var notificationInfos = new List<NotificationInfo>();
        foreach (var notification in user.Notifications)
        {
            var notificationInfo = _mapper.Map<NotificationInfo>(notification);
            notificationInfo.ShortTaskInfo = _mapper.Map<ShortTaskInfo>(notification.Task);
            notificationInfo.ShortProjectInfo = _mapper.Map<ShortProjectInfo>(notification.Project);
            notificationInfo.ShortEventInfo = _mapper.Map<ShortEventInfo>(notification.Event);

            notificationInfos.Add(notificationInfo);
        }

        return notificationInfos;
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
    public async Task<Notification> EventCreated(User user, Event eventId)
    {
        var newNotification = Notification.CreateEventNotification(user, eventId);

        _context.Add(newNotification);
        await _context.SaveChangesAsync();

        _dictionary.AddToDictionary(user.Id, newNotification);

        return newNotification;
    }

    
}