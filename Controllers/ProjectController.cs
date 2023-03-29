using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StasDiplom.Domain;
using StasDiplom.Dto.Project.Requests;
using StasDiplom.Dto.Project.Responses;
using StasDiplom.Dto.Users.Project;
using StasDiplom.Services.Interfaces;
using StasDiplom.Utility;
using MyTask = StasDiplom.Domain.Task;

namespace StasDiplom.Controllers;

[ApiController]
[Route("api/project/")]
public class ProjectController : Controller
{
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly ICalendarService _calendarService;
    private readonly IProjectService _projectService;

    public ProjectController(IMapper mapper,
        INotificationService notificationService, ICalendarService calendarService, IProjectService projectService)
    {
        _mapper = mapper;
        _notificationService = notificationService;
        _calendarService = calendarService;
        _projectService = projectService;
    }

    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateProject(CreateProjectRequest projectRequest)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var newProject = await _projectService.CreateProject(projectRequest, id);

        await _calendarService.CreateProjectCalendar(newProject);

        return Ok(_mapper.Map<CreateProjectResponse>(newProject));
    }

    [Authorize]
    [HttpGet("user-tasks")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAllTasks()
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        return Ok(await _projectService.GetUsersTasks(id));
    }

    [Authorize]
    [HttpGet("")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAllProjects()
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        return Ok(_projectService.GetAllProjects(id));
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

        Project project;
        User user;

        try
        {
            (project, user) = await _projectService.InviteUser(request, id);
        }
        catch (ArgumentException e)
        {
            return e switch
            {
                {Message: "User is not exist"} => NotFound(),
                {Message: "Project is not exist"} => BadRequest(),
                {Message: "User has no permissions"} => Forbid(),
                _ => Problem()
            };
        }

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

        Project project;
        User user;

        try
        {
            (project, user) = await _projectService.KickUser(request, id);
        }
        catch (ArgumentException e)
        {
            return e switch
            {
                {Message: "User is not exist"} => NotFound(),
                {Message: "Project is not exist"} => BadRequest(),
                {Message: "User has no permissions"} => Forbid(),
                _ => Problem()
            };
        }

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

        try
        {
            await _projectService.ChangeRole(request, id);
        }
        catch (ArgumentException e)
        {
            return e switch
            {
                {Message: "User is not exist"} => NotFound(),
                {Message: "Project is not exist"} => BadRequest(),
                {Message: "User has no permissions"} => Forbid(),
                _ => Problem()
            };
        }

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

        try
        {
            return Ok(await _projectService.UpdateProject(request, id));
        }
        catch (ArgumentException e)
        {
            return e switch
            {
                {Message: "User is not exist"} => NotFound(),
                {Message: "Project is not exist"} => BadRequest(),
                {Message: "User has no permissions"} => Forbid(),
                _ => Problem()
            };
        }
    }


    [Authorize]
    [HttpDelete("")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteProject(DeleteRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        try
        {
            return Ok(_projectService.DeleteProject(request, id));
        }
        catch (ArgumentException e)
        {
            return e switch
            {
                {Message: "User is not exist"} => NotFound(),
                {Message: "Project is not exist"} => BadRequest(),
                {Message: "User has no permissions"} => Forbid(),
                _ => Problem()
            };
        }
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

        try
        {
            return Ok(await _projectService.GetProject(projectId, id));
        }
        catch (ArgumentException e)
        {
            return e switch
            {
                {Message: "Project is not exist"} => BadRequest(),
                {Message: "User has no permissions"} => Forbid(),
                _ => Problem()
            };
        }
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

        try
        {
            await _projectService.AcceptInvitation(projectId, id);
        }
        catch (ArgumentException e)
        {
            return e switch
            {
                {Message: "Project is not exist"} => NotFound(),
                {Message: "User is already on project"} => BadRequest(),
                {Message: "User is not invited on project"} => Forbid(),
                _ => Problem()
            };
        }

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

        try
        {
            return Ok(_projectService.GetProjectUsers(projectId, id));
        }
        catch (ArgumentException e)
        {
            return e switch
            {
                {Message: "User is not exist"} => NotFound(),
                {Message: "Project is not exist"} => BadRequest(),
                {Message: "User has no permissions"} => Forbid(),
                _ => Problem()
            };
        }
    }
}