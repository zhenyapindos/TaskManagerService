using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Domain;
using TaskService.Services.Interfaces;
using TaskService.Utility;

namespace TaskService.Controllers;

[ApiController]
[Route("api/calendar/")]
public class CalendarController : Controller
{
    private readonly ProjectManagerContext _context;
    private readonly ICalendarService _calendarService;
    private readonly UserManager<User> _userManager;

    public CalendarController(ICalendarService calendarService, UserManager<User> userManager, ProjectManagerContext context)
    {
        _calendarService = calendarService;
        _userManager = userManager;
        _context = context;
    }

    [Authorize]
    [HttpGet("byUser/{username}")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserCalendar([FromRoute] string username)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        var foundUser = _userManager.FindByNameAsync(username).Result; 
    
        if (foundUser == null)
        {
            return NotFound();
        }

        var currentUser = _userManager.FindByIdAsync(id).Result; 
        
        var project = _context.Projects.Include(x => x.ProjectUsers)
            .ThenInclude(x=> x.User)
            .FirstOrDefault(x=> x.Users.Contains(currentUser) && x.Users.Contains(foundUser));
        
        if (project == null)
        {
            return Forbid();
        }
        
        return Ok(await _calendarService.GetUserCalendar(foundUser.UserName));
    }

    [Authorize]
    [HttpGet("byProject/{projectId:int}")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProjectCalendar([FromRoute] int projectId)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        var user = _userManager.FindByIdAsync(id).Result;
        
        var project = _context.Projects.Include(x=> x.Users).FirstOrDefault(x => x.Id == projectId);

        if (project == null)
        {
            return NotFound();
        }
        
        if (!project.Users.Contains(user))
        {
            return Forbid();
        }
        
        return Ok(await _calendarService.GetProjectCalendar(projectId));
    }
}