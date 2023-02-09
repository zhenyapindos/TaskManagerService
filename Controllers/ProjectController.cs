using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto.Project;
using StasDiplom.Dto.Project.Requests;
using StasDiplom.Dto.Project.Responses;
using StasDiplom.Enum;
using StasDiplom.Utility;
using Task = System.Threading.Tasks.Task;

namespace StasDiplom.Controllers;

[ApiController]
[Route("api/project/")]
public class ProjectController : Controller
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public ProjectController(ProjectManagerContext context, UserManager<User> userManager, IMapper mapper, INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateProject(CreateProjectRequest projectRequest)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var calendar = new Calendar
        {
            Title = projectRequest.Title + "'s calendar"
        };

        var newProject = new Project
        {
            Title = projectRequest.Title,
            Description = projectRequest.Description,
            Calendar = calendar
        };

        await _context.Projects.AddAsync(newProject);

        var projectUser = new ProjectUser
        {
            UserId = id,
            UserProjectRole = UserProjectRole.Admin,
            Project = newProject
        };

        await _context.ProjectUsers.AddAsync(projectUser);

        await _context.SaveChangesAsync();
        
        return Ok(new CreateProjectResponse
        {
            Id = newProject.Id,
            Title = newProject.Title,
            Description = newProject.Description
        });
    }

    [Authorize]
    [HttpGet("")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAllProjects()
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var projectUsers = _mapper.Map<IEnumerable<UserProjectResponse>>(_context
            .ProjectUsers
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .Include(x => x.Project));
            //.ThenInclude(x => x.) свойства из проекта
            
        return Ok(projectUsers);
    }

    [Authorize]
    [HttpPost("invite-user")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> InviteUser([FromBody] UserProjectInteractionRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var user = request.EmailOrUsername.Contains('@') 
            ? await _userManager.FindByEmailAsync(request.EmailOrUsername)
            : await _userManager.FindByNameAsync(request.EmailOrUsername);

        if (user == null) return BadRequest();

        var project = _context.Projects.Include(x => x.ProjectUsers).FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null) return NotFound();

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (resultUser == null) return Forbid();

        var projectUser = new ProjectUser
        {
            UserProjectRole = UserProjectRole.Invited,
            User = user,
            Project = project
        };

        await _context.ProjectUsers.AddAsync(projectUser);
        await _context.SaveChangesAsync();
        
        //ToDo
        //_notificationService.ProjectInvitation(id, projectId);
        
        return Ok();
    }
    
    [Authorize]
    [HttpPost("kick-user")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> KickUser([FromBody] UserProjectInteractionRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var user = request.EmailOrUsername.Contains('@') 
            ? await _userManager.FindByEmailAsync(request.EmailOrUsername)
            : await _userManager.FindByNameAsync(request.EmailOrUsername);

        if (user == null) return BadRequest();

        var project = _context.Projects.Include(x => x.ProjectUsers).FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null) return NotFound();

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (resultUser == null) return Forbid();
        
        resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == user.Id);

        if (resultUser == null) return NotFound();
        
        _context.ProjectUsers.Remove(resultUser);
        await _context.SaveChangesAsync();
        
        //ToDo
        //_notificationService.ProjectInvitation(id, projectId);
        
        return Ok();
    }
    
    [Authorize]
    [HttpPost("change-role")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ChangeRole(UserProjectInteractionRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var user = request.EmailOrUsername.Contains('@') 
            ? await _userManager.FindByEmailAsync(request.EmailOrUsername)
            : await _userManager.FindByNameAsync(request.EmailOrUsername);

        if (user == null) return BadRequest();

        var project = _context.Projects.Include(x => x.ProjectUsers).FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null) return NotFound();

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (resultUser == null) return Forbid();
        
        resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == user.Id);

        if (resultUser == null) return NotFound();

        resultUser.UserProjectRole = resultUser.UserProjectRole == UserProjectRole.Worker 
            ? UserProjectRole.Moderator : UserProjectRole.Worker;
        
        _context.ProjectUsers.Update(resultUser);
        await _context.SaveChangesAsync();
        
        //ToDo
        //_notificationService.ProjectInvitation(id, projectId);
        
        return Ok();
    }
    
    
    /*[HttpGet("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProjectResponse([FromBody] int projectId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        var project = _context.Projects.FirstOrDefault(x => x.Id == projectId);

        if (project == null) return NotFound();

        var result = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (result == null) return Forbid();

        var taskShortInfoList = new List<TaskShortInfo>();

        //HELP
        foreach (var task in project.Tasks)
        {
            taskShortInfoList.Add(new TaskShortInfo
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.CreationTime,
                TaskStatus = task.Tas
            });
        }
        
        var projectResponse = new ProjectResponse
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            TaskList = project.Tasks,
            
        };
        
        return Ok();
    }*/
}