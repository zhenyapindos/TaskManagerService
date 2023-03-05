using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto;
using StasDiplom.Enum;
using Task = StasDiplom.Domain.Task;

namespace StasDiplom.Services;

public class NotificationService : INotificationService
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private Dictionary<string, List<Notification>> _dictionary;
    private List<Notification> _notifications;
    
    public NotificationService(ProjectManagerContext context, UserManager<User> userManager, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _notifications = new List<Notification>();
        _dictionary = new Dictionary<string, List<Notification>>();
    }

    public async Task<Notification> ProjectInvitation(Project project, User user)
    {
        var newNotification = CreateNotificationForProject(project, user);
        newNotification.NotificationType = NotificationType.ProjectInvitation;
    
        _notifications = _context.Notifications.Where(x=> x.User.Id == user.Id).ToList();

        if (!_dictionary.ContainsKey(user.Id))
        {
            _dictionary.Add(user.Id, _notifications);
        }
        else
        {
            _dictionary[user.Id] = _notifications;
        }

        _notifications.Clear();
        
        await _context.SaveChangesAsync();

        return newNotification;
    }

    public async Task<Notification> ProjectKick(Project project, User user)
    {
        var newNotification = CreateNotificationForProject(project, user);
        newNotification.NotificationType = NotificationType.KickedProject;

        _notifications = _context.Notifications.Where(x=> x.User.Id == user.Id).ToList();

        if (!_dictionary.ContainsKey(user.Id))
        {
            _dictionary.Add(user.Id, _notifications);
        }
        else
        {
            _dictionary[user.Id] = _notifications;
        }

        _notifications.Clear();
        
        await _context.SaveChangesAsync();

        return newNotification;
    }

    public async Task<Notification> TaskAssignUser(Task task, User user)
    {
        var newNotification = CreateNotificationForTask(task, user);
        newNotification.NotificationType = NotificationType.TaskAssigment;
       
        _notifications = _context.Notifications.Where(x=> x.User.Id == user.Id).ToList();

        if (!_dictionary.ContainsKey(user.Id))
        {
            _dictionary.Add(user.Id, _notifications);
        }
        else
        {
            _dictionary[user.Id] = _notifications;
        }

        _notifications.Clear();
        
        await _context.SaveChangesAsync();

        return newNotification;
    }

    public async Task<Notification> UserMention(User user, Comment comment)
    {
        var newNotification = CreateNotificationForComment(user, comment);
        newNotification.NotificationType = NotificationType.Mention;
        
        _notifications = _context.Notifications.Where(x=> x.User == user).ToList();

        if (!_dictionary.ContainsKey(user.Id))
        {
            _dictionary.Add(user.Id, _notifications);
        }
        else
        {
            _dictionary[user.Id] = _notifications;
        }

        _notifications.Clear();
        
        await _context.SaveChangesAsync();

        return newNotification;
    }

    public bool IsUnreadNotifications(User user)
    {
        var userNotifications = new List<Notification>();
        /*foreach (var pair in _dictionary)
        {
            if (pair.Key == user)
            {
                userNotifications.Add(pair.Value);
            }
        }*/
        //userNotifications = _context.Notifications.Where(x => x.User == user).ToList();
        //return userNotifications.Any(notification => notification.IsRead == false);
        return _dictionary.Count > 0;
    }

    public async Task<List<NotificationInfo>> GetUnreadNotifications(User user)
    {
        user.Notifications = _context.Notifications.Where(x => x.User == user && x.IsRead == false).ToList();

        return user.Notifications.Select(notification => _mapper.Map<NotificationInfo>(notification)).ToList();
    }
    
    public void MarkAsRead(User user, [FromQuery] int id)
    {
        var notification = _context.Notifications.FirstOrDefault(x => x.User == user && x.Id == id);

        notification.IsRead = true;

        _context.Notifications.Update(notification);
        _context.SaveChanges();
    }

    public async Task<List<NotificationInfo>> GetAllNotificationsInfo(User user)
    {
        user.Notifications = _context.Notifications.Where(x => x.User == user).ToList();

        return user.Notifications.Select(notification => _mapper.Map<NotificationInfo>(notification)).ToList();
    }

    private Notification CreateNotificationForProject(Project project, User user)
    {
        var notification = new Notification
        {
            CreationTime = DateTime.Now,
            IsRead = false,
            Project = project,
            User = user
        };

        _context.Notifications.AddAsync(notification);

        return notification;
    }

    private Notification CreateNotificationForTask(Task task, User user)
    {
        var notification = new Notification
        {
            CreationTime = DateTime.Now,
            IsRead = false,
            Task = task,
            User = user,
            Project = task.Project
        };

        _context.Notifications.AddAsync(notification);

        return notification;
    }

    private Notification CreateNotificationForComment(User user, Comment comment)
    {
        var notification = new Notification
        {
            CreationTime = DateTime.Now,
            IsRead = false,
            User = user,
            Project = comment.Project,
            Task = comment.Task
        };

        _context.Notifications.AddAsync(notification);

        return notification;
    }
}