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
using StasDiplom.Dto.Users;
using StasDiplom.Dto.Users.Task;
using StasDiplom.Enum;
using StasDiplom.Services;
using StasDiplom.Services.Interfaces;
using StasDiplom.Utility;
using Task = System.Threading.Tasks.Task;
using TaskStatus = StasDiplom.Enum.TaskStatus;
using DomainTask = StasDiplom.Domain.Task;

namespace StasDiplom.Controllers;

[ApiController]
[Route("api/task/")]
public class TaskController : Controller
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager; 
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly ITaskService _taskService;
    
    public TaskController(ProjectManagerContext context, UserManager<User> userManager, IMapper mapper, 
        INotificationService service, ITaskService taskService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _notificationService = service;
        _taskService = taskService;
    }

    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        try
        {
            return Ok(await _taskService.CreateTask(request, id));
        }
        catch (ArgumentException e)
        {
            return e switch
            {
                {Message: "Project is not found"} => NotFound(),
                {Message: "User is not in project"} => Forbid(),
                _ => Problem() 
            };
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
    }

    
    [Authorize]
    [HttpGet("/api/task/{taskId:int}")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTaskInfo([FromRoute] int taskId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        try
        {
            return Ok(_taskService.GetTaskInfo(taskId, id));
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
    }

    [Authorize]
    [HttpPut("/api/task/{taskId:int}")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTaskInfo([FromRoute] int taskId, [FromBody] UpdateTaskRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        try
        {
            return Ok(await _taskService.UpdateTask(taskId, request, id));
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
    }

    [Authorize]
    [HttpDelete("/api/task/{taskId:int}")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTask([FromRoute] int taskId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        try
        {
            await _taskService.DeleteTask(taskId, id);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
        
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
        
        DomainTask task;
        User user;

        try
        {
            (task, user) = await _taskService.AssignUser(request, id);
        }
        catch (InvalidOperationException e)
        {
            return e switch
            {
                {Message: "User has no permissions"} => Forbid(),
                {Message: "User is not found"} => NotFound(),
                _ => Problem()
            };
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        
        await _notificationService.TaskAssignUser(task, user);
        
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

        try
        {
            await _taskService.AssignUser(request, id);
        }
        catch (InvalidOperationException e)
        {
            return e switch
            {
                {Message: "User has no permissions"} => Forbid(),
                {Message: "User is not found"} => NotFound(),
                _ => Problem()
            };
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        
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

        try
        {
            await _taskService.MarkTaskAsDone(taskId, id);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
        
        return Ok();
    }
}