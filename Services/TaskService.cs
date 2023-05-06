using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Domain;
using TaskService.Dto.Project;
using TaskService.Dto.Task;
using TaskService.Dto.Users;
using TaskService.Dto.Users.Task;
using TaskService.Enum;
using TaskService.Services.Interfaces;
using Task = System.Threading.Tasks.Task;
using DomainTask = TaskService.Domain.Task;
using TaskStatus = TaskService.Enum.TaskStatus;

namespace TaskService.Services;

public class TaskService : ITaskService
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;
    public TaskService(ProjectManagerContext context, UserManager<User> userManager, IMapper mapper, IEventService eventService, INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _eventService = eventService;
        _notificationService = notificationService;
    }

    public async Task<TaskShortInfo> CreateTask(CreateTaskRequest request, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var project = _context.Projects.Include(x => x.ProjectUsers)
            .FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null)
        {
            throw new ArgumentException("Project is not found");
        }

        var projectUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == user.Id);

        if (projectUser == null)
        {
            throw new ArgumentException("User is not in project");
        }

        if (projectUser.UserProjectRole != UserProjectRole.Admin &&
            projectUser.UserProjectRole != UserProjectRole.Moderator)
        {
            throw new InvalidOperationException("User has no permissions");
        }

        var newTask = _mapper.Map<DomainTask>(request) with
        {
            CreationTime = DateTime.UtcNow,
            Project = project
        };

        if (request.ParentTaskId != null)
        {
            newTask.ParentTaskId = request.ParentTaskId;
        }

        if (request.PreviousTaskId != null)
        {
            newTask.PreviousTaskId = request.PreviousTaskId;

            newTask.ParentTaskId = _context.Tasks.FirstOrDefault(x => x.Id == request.PreviousTaskId).ParentTaskId;
        }

        if (request.StartDate != null)
        {
            newTask.Deadline = request.StartDate.Value.AddHours(request.DurationHours);
        }

        if (request.StartDate == null)
        {
            newTask.TaskStatus = TaskStatus.Created;
            newTask.Deadline = null;
        }

        if (newTask.StartDate > DateTime.UtcNow)
        {
            newTask.TaskStatus = TaskStatus.Planned;
        }
        else if (DateTime.UtcNow < newTask.Deadline && DateTime.UtcNow >= newTask.StartDate)
        {
            newTask.TaskStatus = TaskStatus.InProgress;
        }
        else if (newTask.Deadline < DateTime.UtcNow)
        {
            newTask.TaskStatus = TaskStatus.Overdue;
        }

        await _context.Tasks.AddAsync(newTask);
        await _context.SaveChangesAsync();

        var taskUsers = new List<TaskUser>
        {
            new()
            {
                User = user,
                Task = newTask,
                TaskRole = TaskRole.Creator
            }
        };

        if (request.AssignedUsersUsernames != null)
        {
            foreach (var username in request.AssignedUsersUsernames)
            {
                user = await _userManager.FindByNameAsync(username);

                if (user == null) continue;

                var taskUser = new TaskUser
                {
                    Task = newTask,
                    TaskRole = TaskRole.Assigned,
                    User = user
                };

                await _notificationService.TaskAssignUser(newTask, user);
                taskUsers.Add(taskUser);
            }
        }

        await _context.TaskUsers.AddRangeAsync(taskUsers);

        await _context.SaveChangesAsync();

        return _mapper.Map<TaskShortInfo>(newTask);
    }

    public async Task<TaskInfoResponse> GetTaskInfo(int taskId, string userId)
    {
        var task = _context.Tasks
            .Include(x => x.Project)
            .ThenInclude(x => x.ProjectUsers)
            .Include(x => x.TaskUsers)
            .FirstOrDefault(x => x.Id == taskId);

        if (task == null)
        {
            throw new ArgumentException("Task is not found");
        }

        var taskUser = task.Project.ProjectUsers.FirstOrDefault(x => x.UserId == userId);

        if (taskUser == null)
        {
            throw new InvalidOperationException("User has no permission");
        }

        var project = _context.Projects.FirstOrDefault(x => x.Id == task.ProjectId);
        var shortProjectInfo = _mapper.Map<ShortProjectInfo>(project);

        var response = _mapper.Map<TaskInfoResponse>(task);
        response.Project = shortProjectInfo;

        if (task.StartDate != null)
        {
            response.Deadline = task.StartDate!.Value.AddHours((double) task.DurationHours!);
        }

        if (task.StartDate == null && task.TaskStatus != Enum.TaskStatus.Done)
        {
            response.Deadline = null;
            response.Status = (TaskStatus) Enum.TaskStatus.Created;
        }
        else if (task.TaskStatus != Enum.TaskStatus.Done)
        {
            if (task.StartDate > DateTime.UtcNow)
            {
                response.Status = (TaskStatus) 2;
            }
            else if (DateTime.UtcNow < response.Deadline && DateTime.UtcNow >= task.StartDate)
            {
                response.Status = (TaskStatus) 1;
            }
            else if (response.Deadline < DateTime.UtcNow)
            {
                response.Status = 0;
            }
        }
        else
        {
            response.Status = (TaskStatus) Enum.TaskStatus.Done;
            //mappedTask.Deadline = task.StartDate!.Value.AddHours((double) task.DurationHours!);   response.Deadline = null;
        }

        if (task.PreviousTaskId != null)
        {
            response.PreviousTask =
                _mapper.Map<TaskShortInfo>(_context.Tasks.FirstOrDefault(x => x.Id == task.PreviousTaskId));
        }

        if (task.ParentTaskId != null)
        {
            response.ParentTask =
                _mapper.Map<TaskShortInfo>(_context.Tasks.FirstOrDefault(x => x.Id == task.ParentTaskId));
        }

        var users = task.TaskUsers.Join(_userManager.Users,
                pu => pu.UserId, u => u.Id,
                (projectUser, taskUser) => _mapper.Map<UserShortInfo>((taskUser, projectUser)))
            .ToList();

        response.AssignedUsers = users;

        return response;
    }

    public async Task<TaskInfoResponse> UpdateTask(int taskId, UpdateTaskRequest request, string id)
    {
        var task = _context.Tasks
            .AsNoTracking()
            .Include(x => x.TaskUsers)
            .Include(x => x.Project)
            .ThenInclude(x => x.ProjectUsers)
            .FirstOrDefault(x => x.Id == taskId);

        if (task == null)
        {
            throw new ArgumentException("Task is not found");
        }

        var user = task.Project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (user == null)
        {
            throw new InvalidOperationException("User has no permissions");
        }

        if (user.UserProjectRole != UserProjectRole.Admin && user.UserProjectRole != UserProjectRole.Moderator)
        {
            throw new InvalidOperationException("User has no permissions");
        }

        if (request.Title != null)
        {
            task.Title = request.Title;
        }
        
        if (request.Description != null)
        {
            task.Description = request.Description;
        }
        
        if (request.StartDate != null)
        {
            task.StartDate = request.StartDate;
        }
        
        if (request.DurationHours != null)
        {
            task.DurationHours = request.DurationHours;
        }

        if (task.StartDate != null)
        {
            task.Deadline = task.StartDate!.Value.AddHours((double) task.DurationHours!);
        }

        if (task.StartDate == null && task.TaskStatus != Enum.TaskStatus.Done)
        {
            task.Deadline = null;
            task.TaskStatus = TaskStatus.Created;
        }
        else if (task.TaskStatus != TaskStatus.Done)
        {
            if (task.StartDate > DateTime.UtcNow)
            {
                task.TaskStatus = (TaskStatus) 2;
            }
            else if (DateTime.UtcNow < task.Deadline && DateTime.UtcNow >= task.StartDate)
            {
                task.TaskStatus = (TaskStatus) 1;
            }
            else if (task.Deadline < DateTime.UtcNow)
            {
                task.TaskStatus = 0;
            }
        }
        else
        {
            task.TaskStatus = TaskStatus.Done;
            task.Deadline = null;
        }
        
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<TaskInfoResponse>(task);
        response.AssignedUsers = task.TaskUsers.Join(_userManager.Users,
                pu => pu.UserId, u => u.Id, (projectUser, user) => _mapper.Map<UserShortInfo>((user, projectUser)))
            .ToList();

        return response;
    }

    public async Task DeleteTask(int taskId, string userId)
    {
        var task = _context.Tasks
            .Include(x => x.TaskUsers)
            .Include(x => x.Project)
            .ThenInclude(x => x.ProjectUsers)
            .Include(x=> x.Events)
            .FirstOrDefault(x => x.Id == taskId);

        if (task == null)
        {
            throw new ArgumentException("Task is not found");
        }

        var user = task.TaskUsers.FirstOrDefault(x => x.UserId == userId);

        if (user == null)
        {
            throw new InvalidOperationException("User has no permissions");
        }

        var projectUser = task.Project.ProjectUsers.FirstOrDefault(x => x.UserId == userId);

        if (projectUser.UserProjectRole != UserProjectRole.Admin &&
            projectUser.UserProjectRole != UserProjectRole.Moderator)
        {
            throw new InvalidOperationException("User has no permissions");
        }

        var previousTasks = _context.Tasks
            .Where(x => x.PreviousTaskId! == taskId)
            .Include(x => x.Events);
        DeletePreviousTasks(previousTasks);

        var childTasks = _context.Tasks
            .Where(x => x.ParentTaskId! == taskId)
            .Include(x => x.Events);
        DeleteChildTasks(childTasks);

        _context.RemoveRange(_context.Notifications.Where(x=> x.Task == task));
        _context.RemoveRange(_context.Events.Where(x=> x.EventType == EventType.TaskEvent && x.Task == task));
        _context.Remove(task);

        await _context.SaveChangesAsync();
    }

    private async void DeletePreviousTasks(IEnumerable<DomainTask> previousTasks)
    {
        foreach (var task in previousTasks)
        {
            var childTasks = _context.Tasks.Where(x => x.ParentTaskId! == task.Id);
            var eventList = _context.Events.Where(x => x.EventType == EventType.TaskEvent && x.Task == task);
            var notificationList = _context.Notifications.Where(x => x.Task == task);
            
            foreach (var events in eventList)
            {
                _context.Events.Remove(events);
            }
            
            DeleteChildTasks(childTasks);

            _context.RemoveRange(notificationList);
            _context.RemoveRange(eventList);
            task.PreviousTaskId = null;
        }
    }

    private async void DeleteChildTasks(IEnumerable<DomainTask> childTasks)
    {
        foreach (var task in childTasks)
        {
            var previousTasks = _context.Tasks.Where(x => x.PreviousTaskId! == task.Id);
            var eventList = _context.Events.Where(x => x.EventType == EventType.TaskEvent && x.Task == task);
            var notificationList = _context.Notifications.Where(x => x.Task == task);
            
            foreach (var events in eventList)
            {
                _context.Events.Remove(events);
            }

            _context.RemoveRange(notificationList);
            
            DeletePreviousTasks(previousTasks);

            var currentChildTasks = _context.Tasks.Where(x => x.ParentTaskId! == task.Id);
            DeleteChildTasks(currentChildTasks);
            
            _context.Remove(task);
        }
    }

    public async Task<(DomainTask task, User user)> AssignUser(UserTaskInterractionRequest request, string userId)
    {
        var task = _context.Tasks
            .Include(x => x.TaskUsers)
            .Include(x => x.Project)
            .ThenInclude(x => x.ProjectUsers)
            .Include(x => x.TaskUsers)
            .FirstOrDefault(x => x.Id == request.TaskId);

        if (task == null)
        {
            throw new ArgumentException("Task is not found");
        }

        var user = task.TaskUsers.FirstOrDefault(x => x.UserId == userId);

        if (user == null || user.TaskRole != TaskRole.Creator)
        {
            throw new InvalidOperationException("User has no permissions");
        }

        var userExists = await _userManager.FindByNameAsync(request.Username);

        if (userExists == null)
        {
            throw new InvalidOperationException("User is not found");
        }

        var newTaskUser = new TaskUser
        {
            User = userExists,
            Task = task,
            TaskRole = TaskRole.Assigned
        };

        task.TaskUsers.Add(newTaskUser);

        await _context.SaveChangesAsync();

        return (task, userExists);
    }

    public async Task UnassignUser(UserTaskInterractionRequest request, string userId)
    {
        var task = _context.Tasks
            .Include(x => x.TaskUsers)
            .Include(x => x.Project)
            .ThenInclude(x => x.ProjectUsers)
            .Include(x => x.TaskUsers)
            .ThenInclude(x=> x.User)
            .FirstOrDefault(x => x.Id == request.TaskId);

        if (task == null)
        {
            throw new ArgumentException("Task is not found");
        }

        var user = task.TaskUsers.FirstOrDefault(x => x.UserId == userId);

        if (user == null || user.TaskRole != TaskRole.Creator)
        {
            throw new InvalidOperationException("User has no permissions");
        }

        var userExists = await _userManager.FindByNameAsync(request.Username);

        if (userExists == null)
        {
            throw new InvalidOperationException("User is not found");
        }

        var taskUser = _context.TaskUsers.FirstOrDefault(x => x.User == userExists);
        
        if (taskUser != null)
        {
            _context.TaskUsers.Remove(taskUser);
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkTaskAsDone(int taskId, string userId)
    {
        var task = _context.Tasks
            .Include(x => x.TaskUsers)
            .FirstOrDefault(x => x.Id == taskId);

        if (task == null)
        {
            throw new ArgumentException("Task is not found");
        }

        var user = task.TaskUsers.FirstOrDefault(x => x.UserId == userId);

        if (user == null || user.TaskRole != TaskRole.Creator)
        {
            throw new InvalidOperationException("User has no permissions");
        }

        task.TaskStatus = TaskStatus.Done;

        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public bool IsTaskAsEvent(int taskId, string userId)
    {
        var task = _context.Tasks.FirstOrDefault(x => x.Id == taskId);

        return _context.Events.FirstOrDefault(x => x.Task == task) != null;
    }
}