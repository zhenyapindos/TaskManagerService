using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Domain;
using TaskService.Dto.Event;
using TaskService.Dto.Users.Event;
using TaskService.Enum;
using TaskService.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskService.Services;

public class EventService : IEventService
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly ILogger<EventService> _logger;

    public EventService(ProjectManagerContext context, IMapper mapper, UserManager<User> userManager, 
        INotificationService notificationService, ILogger<EventService> logger)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<EventInfo> GetEventInfo(int eventId, string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            throw new ArgumentNullException();
        }
        
        var findedEvent = _context.Events
            .Include(x=> x.Calendar)
            .FirstOrDefault(x => x.Id == eventId);
        
        var eventUsernames = _context.EventUsers
            .Where(x => x.EventId == findedEvent.Id)
            .Select(x=> x.User.UserName).ToList();
        
        return _mapper.Map<EventInfo>(findedEvent) with
        {
            CreatorUsername = user.UserName,
            AssignedUsernames = eventUsernames
        };
    }
    public async Task<EventInfo> CreateEvent(CreateEventRequest request, string id)
    {
        var calendar = _context.Calendars.FirstOrDefault(x => x.Id == request.CalendarId);

        if (calendar == null || (calendar.UserId != null && request.Usernames != null))
        {
            throw new ArgumentException();
        }

        if (calendar.UserId != null && calendar.UserId != id)
        {
            throw new InvalidOperationException();
        }

        if (calendar.ProjectId != null)
        {
            var projectUsers = _context.ProjectUsers.AsNoTracking()
                .Where(x => x.ProjectId == calendar.ProjectId);

            if (!projectUsers.Any(x => x.UserId == id))
            {
                throw new InvalidOperationException();
            }

            if (request.Usernames != null)
            {
                foreach (var username in request.Usernames)
                {
                    var user = await _userManager.FindByNameAsync(username);

                    if (user == null)
                    {
                        continue;
                    }

                    if (!projectUsers.Any(x => x.UserId == user.Id))
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        var newEvent = _mapper.Map<Event>(request) with
        {
            CreatorId = id
        };

        if (request.MeetingLink == null)
        {
            newEvent.EventType = (EventType) 2;
        }
        else
        {
            newEvent.EventType = (EventType) 0;
        }

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        if (request.Usernames != null)
        {
            foreach (var username in request.Usernames)
            {
                var user = await _userManager.FindByNameAsync(username);

                var eventUser = new EventUser()
                {
                    Event = newEvent,
                    User = user,
                    EventType = newEvent.EventType
                };

                await _notificationService.EventCreated(user, newEvent);
                _context.EventUsers.Add(eventUser);
            }
        }

        var currentUser = _userManager.FindByIdAsync(id).Result;
        //await _notificationService.EventCreated(currentUser, newEvent);

        var currentEventUser = new EventUser()
        {
            User = currentUser,
            Event = newEvent,
            EventType = newEvent.EventType
        };

        _context.EventUsers.Add(currentEventUser);

        await _context.SaveChangesAsync();

        var eventUsers = _context.EventUsers.Where(x => x.EventId == newEvent.Id).ToList();

        return (_mapper.Map<EventInfo>(newEvent) with
        {
            CreatorUsername = (await _userManager.FindByIdAsync(id)).UserName,
            AssignedUsernames = eventUsers.Select(x => x.User.UserName).ToList()
        });
    }

    public async Task<EventInfo> UpdateEvent(UpdateEventRequest request, string id)
    {
        var oldEvent = _context.Events
            .Include(x => x.EventUsers)
            .FirstOrDefault(x => x.Id == request.Id);

        if (oldEvent == null)
        {
            throw new ArgumentException();
        }

        var user = oldEvent.EventUsers.FirstOrDefault(x => x.UserId == id);

        if (user == null && oldEvent.CreatorId != id)
        {
            throw new InvalidOperationException();
        }

        if (request.Title != null)
        {
            oldEvent.Title = request.Title;
        }
        
        if (request.Description != null)
        {
            oldEvent.Description = request.Description;
        }

        if (request.MeetingLink != null)
        {
            oldEvent.MeetingLink = request.MeetingLink;
            oldEvent.EventType = (EventType) 0;
        }
        else
        {
            oldEvent.EventType = EventType.Other;
        }

        _context.Events.Update(oldEvent);
        await _context.SaveChangesAsync();

        var eventUsers = _context.EventUsers
            .Where(x => x.EventId == oldEvent.Id)
            .Select(x=>x.User.UserName).ToList();

        var info = _mapper.Map<EventInfo>(oldEvent) with
        {
            CreatorUsername = (await _userManager.FindByIdAsync(id)).UserName,
            AssignedUsernames = eventUsers
        };

        return info;
    }

    public async Task DeleteEvent(int eventId, string id)
    {
        var eventForDeleting = _context.Events
            .Include(x => x.EventUsers)
            .FirstOrDefault(x => x.Id == eventId);

        var notifications = _context.Notifications.Where(x => x.EventId == eventId);
        
        if (eventForDeleting == null)
        {
            throw new ArgumentException();
        }

        var user = eventForDeleting.EventUsers.FirstOrDefault(x => x.UserId == id);

        if (user == null && eventForDeleting.CreatorId != id)
        {
            throw new InvalidOperationException();
        }
        
        _context.RemoveRange(notifications);
        _context.Events.Remove(eventForDeleting);

        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task AssignUser(UserEventInteractionRequest request, string id)
    {
        var eventForAssigment = _context.Events.Include(x => x.EventUsers)
            .FirstOrDefault(x => x.Id == request.EventId);

        if (eventForAssigment == null)
        {
            throw new ArgumentException();
        }

        var addingUser = await _userManager.FindByNameAsync(request.Username);

        if (addingUser == null)
        {
            throw new ArgumentException();
        }

        var calendar = _context.Calendars.FirstOrDefault(x => x.Id == eventForAssigment.CalendarId);

        if (calendar == null)
        {
            throw new InvalidOperationException();
        }

        var userList = _context.ProjectUsers.Include(x => x.User)
            .Where(x => x.Project.Calendar == calendar).Select(x => x.User).ToList();

        if (!userList.All(x => x.UserName != addingUser.UserName))
        {
            throw new InvalidOperationException();
        }

        if (userList.All(x => x.Id != id))
        {
            throw new InvalidOperationException();
        }

        if (eventForAssigment.EventUsers.FirstOrDefault(x => x.User.UserName == addingUser.UserName) != null)
        {
            throw new InvalidOperationException();
        }

        if (eventForAssigment.CreatorId != id)
        {
            throw new InvalidOperationException();
        }

        var newEventUser = new EventUser()
        {
            Event = eventForAssigment,
            User = addingUser,
            EventType = eventForAssigment.EventType
        };

        _context.Events.Update(eventForAssigment);
        eventForAssigment.EventUsers.Add(newEventUser);
        _context.EventUsers.Add(newEventUser);
        await _notificationService.EventCreated(addingUser, eventForAssigment);

        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task UnassignUser(UserEventInteractionRequest request, string id)
    {
        var eventForUnassigment = _context.Events.Include(x => x.EventUsers)
            .FirstOrDefault(x => x.Id == request.EventId);

        if (eventForUnassigment == null)
        {
            throw new ArgumentException();
        }

        if (eventForUnassigment.CreatorId != id)
        {
            throw new InvalidOperationException();
        }

        var deletingUser = await _userManager.FindByNameAsync(request.Username);

        if (deletingUser == null || eventForUnassigment.CreatorId == deletingUser.Id)
        {
            throw new ArgumentException();
        }

        var calendar = _context.Calendars.FirstOrDefault(x => x.Id == eventForUnassigment.CalendarId);

        if (calendar == null)
        {
            throw new InvalidOperationException();
        }

        var userList = _context.ProjectUsers.Include(x => x.User)
            .Where(x => x.Project.Calendar == calendar).Select(x => x.User).ToList();

        if (!userList.All(x => x.UserName != deletingUser.UserName))
        {
            throw new InvalidOperationException();
        }

        if (userList.All(x => x.Id != id))
        {
            throw new InvalidOperationException();
        }

        if (eventForUnassigment.EventUsers.FirstOrDefault(x => x.User.UserName == deletingUser.UserName) == null)
        {
            throw new InvalidOperationException();
        }

        if (eventForUnassigment.CreatorId != id)
        {
            throw new InvalidOperationException();
        }

        var eventUnassignedUser = _context.EventUsers.FirstOrDefault(x => x.User == deletingUser);

        if (eventUnassignedUser == null)
        {
            throw new ArgumentException();
        }

        eventForUnassigment.EventUsers.Remove(eventUnassignedUser);
        _context.EventUsers.Remove(eventUnassignedUser);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task PostTaskAsEvent(int taskId, string userId)
    {
        var user = _userManager.FindByIdAsync(userId).Result;

        var taskForEvent = _context.Tasks
            .Include(x => x.Project)
            .ThenInclude(x => x.Calendar)
            .FirstOrDefault(x => x.Id == taskId);

        if (taskForEvent.StartDate == null || taskForEvent.Deadline == null)
        {
            throw new ArgumentNullException();
        }

        var taskEvent = new Event
        {
            Title = taskForEvent.Title + "'s task event",
            Calendar = taskForEvent.Project.Calendar,
            EventType = (EventType) 1,
            Start = taskForEvent.StartDate.GetValueOrDefault(),
            End = taskForEvent.Deadline.GetValueOrDefault(),
            CreatorId = userId,
            Task = taskForEvent
        };

        
        _context.Events.Add(taskEvent);
        await _context.SaveChangesAsync();
        
        await _notificationService.EventCreated(user, taskEvent);
        
        var newEventUser = new EventUser()
        {
            Event = taskEvent,
            User = user,
            EventType = EventType.TaskEvent
        };

        _context.EventUsers.Add(newEventUser);
        await _context.SaveChangesAsync();
    }
}