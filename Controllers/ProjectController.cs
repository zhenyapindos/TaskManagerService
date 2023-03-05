using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Utils.Windows;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto.Project;
using StasDiplom.Dto.Project.Requests;
using StasDiplom.Dto.Project.Responses;
using StasDiplom.Enum;
using StasDiplom.Extensions;
using StasDiplom.Services;
using StasDiplom.Utility;
using Task = System.Threading.Tasks.Task;
using MyTask = StasDiplom.Domain.Task;
using TaskStatus = System.Threading.Tasks.TaskStatus;

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

        var newProject = _mapper.Map<Project>(projectRequest);
        newProject.Calendar = calendar;

        await _context.Projects.AddAsync(newProject);

        var projectUser = new ProjectUser
        {
            UserId = id,
            UserProjectRole = UserProjectRole.Admin,
            Project = newProject
        };
        
        await _context.ProjectUsers.AddAsync(projectUser);

        await _context.SaveChangesAsync();
        
        return Ok(_mapper.Map<CreateProjectResponse>(newProject));
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

        var project = _context.Projects
            .Include(x=> x.Notifications)
            .FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null) return NotFound();

        var resultUser = _context.ProjectUsers
            .AsNoTracking()
            .FirstOrDefault(x => x.UserId == id && x.ProjectId == project.Id);
        
        if (resultUser == null) return Forbid();

        var projectUser = new ProjectUser
        {
            UserProjectRole = UserProjectRole.Invited,
            User = user,
            Project = project
        };

        await _context.ProjectUsers.AddAsync(projectUser);
        await _context.SaveChangesAsync();
        await _notificationService.ProjectInvitation(project, user);
        
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
        await _notificationService.ProjectKick(project, user);
        
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

        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null) return NotFound();

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (resultUser == null) return Forbid();
        
        resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == user.Id);

        if (resultUser == null) return NotFound();

        resultUser.UserProjectRole = resultUser.UserProjectRole == UserProjectRole.Worker 
            ? UserProjectRole.Moderator : UserProjectRole.Worker;
        
        _context.ProjectUsers.Update(resultUser);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpPut("")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProject(UpdateProjectRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(y => y.Id == request.Id);

        if (project == null) return NotFound();

        var resultUser = project.ProjectUsers
            .FirstOrDefault(x => x.UserId == id && x.UserProjectRole == UserProjectRole.Admin);
            
        if (resultUser == null) return Forbid();    

        if (request.Title != null)
        {
            project.Title = request.Title;
        }

        if (request.Description != null)
        {
            project.Description = request.Description;
        }

        _context.Projects.Update(project);
        await _context.SaveChangesAsync();

        return Ok(new UpdateProjectResponse
        {
            Title = project.Title,
            Description = project.Description
        });
    }


    [Authorize]
    [HttpDelete("")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteProject(DeleteRequest projectId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(y => y.Id == projectId.Id);

        if (project == null) return NotFound();

        var resultUser = project.ProjectUsers
            .FirstOrDefault(x => x.UserId == id && x.UserProjectRole == UserProjectRole.Admin);
            
        if (resultUser == null) return Forbid();

        _context.Remove(project);
        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [Authorize]
    [HttpGet("{projectId:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProject([FromRoute] int projectId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var project = _context.Projects.Include(x => x.ProjectUsers)
            .Include(x=> x.Tasks)
            .FirstOrDefault(x => x.Id == projectId);

        if (project == null) return NotFound();

        var response = _mapper.Map<GetProjectResponse>(project);
        
        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (resultUser == null) return Forbid();

        var users = project.ProjectUsers.Join(_userManager.Users,
            pu => pu.UserId, u => u.Id, (projectUser, user) => _mapper.Map<UserShortInfo>((user, projectUser))).ToList();

        response.UserList = users;
        response.UserProjectRole = resultUser.UserProjectRole;
        response.TaskList = new List<TaskShortInfo>();

        foreach (var task in project.Tasks)
        {
            response.TaskList.Add(_mapper.Map<TaskShortInfo>(task));
        }
        
        return Ok(response);
    }

    [Authorize]
    [HttpPost("/accept-invitation/{projectId:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AcceptInvitation([FromRoute] int projectId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(y => y.Id == projectId);

        if (project == null) return NotFound();

        var resultUser = project.ProjectUsers
            .FirstOrDefault(x => x.UserId == id);
            
        if (resultUser == null) return Forbid();

        if (resultUser.UserProjectRole != UserProjectRole.Invited) return BadRequest();

        resultUser.UserProjectRole = UserProjectRole.Worker;

        await _context.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpPost("/{projectId:int}/users")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProjectUsers([FromRoute] int projectId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        var project = _context.Projects
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(x => x.Id == projectId);

        if (project == null) return NotFound();

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (resultUser == null) return Forbid();
        
        var users = project.ProjectUsers.Join(_userManager.Users,
            pu => pu.UserId, u => u.Id, (projectUser, user) => _mapper.Map<UserShortInfo>((user, projectUser))).ToList();

        return Ok(users);
    }
}