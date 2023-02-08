using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto.Project;
using StasDiplom.Enum;
using StasDiplom.Utility;

namespace StasDiplom.Controllers;

[ApiController]
[Route("api/project/")]
public class ProjectController : Controller
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager;

    public ProjectController(ProjectManagerContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateProjectRequest(CreateProjectRequest projectRequest)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        /*var calendar = new Calendar
        {
            Title = projectRequest.Title + "'s calendar"
        };*/

        var newProject = new Project
        {
            Title = projectRequest.Title,
            Description = projectRequest.Description
            //CalendarId = calendar.Id
        };

        await _context.Projects.AddAsync(newProject);
        
        //calendar.ProjectId = newProject.Id;

        //await _context.Calendars.AddAsync(calendar);
        
        /*var projectUser = new ProjectUser
        {
            UserId = id,
            UserProjectRole = UserProjectRole.Admin
        };

        await _context.ProjectUsers.AddAsync(projectUser);*/
        
        await _context.SaveChangesAsync();
        
        return Ok();
    }

    [Authorize]
    [HttpGet("")]
    [ProducesResponseType(200)]
    public async Task<List<Project>> GetAllProjects()
    {
        //тот ли эксепшн?
        var user = _userManager.Users.FirstOrDefault(x => x.Id == MyClaims.Id) ?? throw new ArgumentException();

        var projectList = _context.Projects.Where(project => user.Id == project.Calendar.UserId).ToList();

        return projectList;
    }
    
}