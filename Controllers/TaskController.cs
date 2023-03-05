using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto.Project;
using StasDiplom.Dto.Project.Responses;
using StasDiplom.Dto.Task;
using StasDiplom.Enum;
using StasDiplom.Services;
using StasDiplom.Utility;
using Task = StasDiplom.Domain.Task;
using TaskStatus = StasDiplom.Enum.TaskStatus;

namespace StasDiplom.Controllers;

[ApiController]
[Route("api/task/")]
public class TaskController : Controller
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager; 
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    
    public TaskController(ProjectManagerContext context, UserManager<User> userManager, IMapper mapper, INotificationService service)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _notificationService = service;
    }

    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateTask(CreateTaskRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        var user = await _userManager.FindByIdAsync(id);
        
        var newTask = _mapper.Map<Task>(request) with
        {
            CreationTime = DateTime.Now,
            Project = _context.Projects.First(x=> x.Id == request.ProjectId),
            TaskStatus = TaskStatus.Created
        };

        await _context.Tasks.AddAsync(newTask);

        var taskUsers = new List<TaskUser>
        {
            new()
            {
                User = user,
                Task = newTask,
                TaskRole = TaskRole.Creator
            }
        };
        
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
            
            taskUsers.Add(taskUser);
        }

        await _context.TaskUsers.AddRangeAsync(taskUsers);
        
        await _context.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpGet("/api/task/{taskId:int}")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTaskInfo([FromRoute] int taskId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        var task = _context.Tasks
            .Include(x => x.Project)
            .Include(x => x.TaskUsers)
            .FirstOrDefault(x => x.Id == taskId);

        if (task == null) return NotFound();

        var user = task.TaskUsers.FirstOrDefault(x => x.UserId == id);

        if (user == null) return Forbid();

        var project = _context.Projects.FirstOrDefault(x => x.Id == task.ProjectId);
        var shortProjectInfo = _mapper.Map<ShortProjectInfo>(project);

        var response = _mapper.Map<TaskInfoResponse>(task);
        response.Project = shortProjectInfo;

        var users = task.TaskUsers.Join(_userManager.Users,
            pu => pu.UserId, u => u.Id, (projectUser, user) => _mapper.Map<UserShortInfo>((user, projectUser))).ToList();

        response.AssignedUsers = users;
        
        return Ok(response);
    }

    [Authorize]
    [HttpPut("/api/task/{taskId:int}")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTaskInfo([FromRoute] int taskId, [FromBody] UpdateTaskRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var task = _context.Tasks
            .AsNoTracking()
            .Include(x => x.TaskUsers)
            .Include(x=> x.Project)
            .ThenInclude(x=> x.ProjectUsers)
            .FirstOrDefault(x => x.Id == taskId);
        
        if (task == null) return NotFound();

        var projectId = task.ProjectId;
        
        var user = task.Project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (user == null) return Forbid();

        var newTask = _mapper.Map<Task>(request) with
        {
            Id = taskId,
            ProjectId = projectId
        };

        _context.Tasks.Update(newTask);
        await _context.SaveChangesAsync();
        
        var response = _mapper.Map<TaskInfoResponse>(newTask);
        response.AssignedUsers = task.TaskUsers.Join(_userManager.Users,
            pu => pu.UserId, u => u.Id, (projectUser, user) => _mapper.Map<UserShortInfo>((user, projectUser))).ToList();
        
        return Ok(response);
    }

    [Authorize]
    [HttpDelete("/api/task/{taskId:int}")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTask([FromRoute] int taskId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var task = _context.Tasks
            .Include(x => x.TaskUsers)
            .Include(x => x.Project)
            .Include(x => x.TaskUsers)
            .FirstOrDefault(x => x.Id == taskId);

        if (task == null) return NotFound();

        var user = task.TaskUsers.FirstOrDefault(x => x.UserId == id && x.TaskRole == TaskRole.Creator);

        if (user == null) return Forbid();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        
        return Ok();
    }

    [Authorize]
    [HttpPost("/api/task/assign-user")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignUser([FromBody] UserTaskInterractionRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var task = _context.Tasks
            .Include(x => x.TaskUsers)
            .Include(x => x.Project)
            .ThenInclude(x=> x.ProjectUsers)
            .Include(x => x.TaskUsers)
            .FirstOrDefault(x => x.Id == request.TaskId);

        if (task == null) return NotFound();
        
        var user = task.TaskUsers.FirstOrDefault(x => x.UserId == id);

        if (user == null || user.TaskRole != TaskRole.Creator) return Forbid();

        var userExists = await _userManager.FindByNameAsync(request.Username);

        if (userExists == null) return NotFound();

        var newTaskUser = new TaskUser
        {
            User = userExists,
            Task = task,
            TaskRole = TaskRole.Assigned
        };
        
        task.TaskUsers.Add(newTaskUser);
        
        await _context.SaveChangesAsync();
        await _notificationService.TaskAssignUser(task, userExists);
        
        return Ok();
    }
    
    [Authorize]
    [HttpPost("/api/task/unassign-user")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnassignUser([FromBody] UserTaskInterractionRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var task = _context.Tasks
            .Include(x => x.TaskUsers)
            .Include(x => x.Project)
            .ThenInclude(x=> x.ProjectUsers)
            .Include(x => x.TaskUsers)
            .FirstOrDefault(x => x.Id == request.TaskId);

        if (task == null) return NotFound();
        
        var user = task.TaskUsers.FirstOrDefault(x => x.UserId == id);

        if (user == null || user.TaskRole != TaskRole.Creator) return Forbid();

        var userExists = await _userManager.FindByNameAsync(request.Username);

        if (userExists == null) return NotFound();

        var newTaskUser = task.TaskUsers.FirstOrDefault(x => x.UserId == userExists.Id);

        if (newTaskUser != null)
        {
            _context.TaskUsers.Remove(newTaskUser);
        }
        
        await _context.SaveChangesAsync();
        //notificationService
        
        return Ok();
    }
    
    [Authorize]
    [HttpPut("/api/task/{taskId}/done")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkTaskAsDone([FromRoute] int taskId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var task = _context.Tasks
            .Include(x => x.TaskUsers)
            .FirstOrDefault(x => x.Id == taskId);

        if (task == null) return NotFound();
        
        var user = task.TaskUsers.FirstOrDefault(x => x.UserId == id);

        if (user == null || user.TaskRole != TaskRole.Creator) return Forbid();

        task.TaskStatus = TaskStatus.Done;

        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
        
        return Ok();
    }
}