﻿using TaskService.Domain;
using TaskService.Dto.Notification;
using Task = TaskService.Domain.Task;

namespace TaskService.Services.Interfaces;

public interface INotificationService
{
    public Task<Notification> ProjectInvitation(Project project, User user);
    public Task<Notification> ProjectKick(Project project, User user);
    public Task<Notification> TaskAssignUser(Task task, User user);
    public Task<Notification> UserMention(User user, Comment comment);
    public bool IsUnreadNotifications(User user);
    public Task<List<NotificationInfo>> GetUnreadNotifications(User user);
    public void MarkAsRead(User user, List<int> ids);
    public Task<List<NotificationInfo>> GetAllNotificationsInfo(User user);
    public Task<Notification> EventCreated(User user, Event eventId);
}