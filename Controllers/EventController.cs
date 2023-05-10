using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskService.Context;
using TaskService.Domain;
using TaskService.Dto.Event;
using TaskService.Dto.Users.Event;
using TaskService.Services.Interfaces;
using TaskService.Utility;

namespace TaskService.Controllers;


[ApiController]
[Route("api/event/")]
public class EventController : Controller
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        EventInfo info;
        
        try
        {
            info = await _eventService.CreateEvent(request, id);
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }

        return Ok(info);
    }

    [Authorize]
    [HttpPut("")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateEvent([FromBody] UpdateEventRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        EventInfo info;
        
        try
        {
            info = await _eventService.UpdateEvent(request, id);
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }

        return Ok(info);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeleteEvent([FromRoute] int id)
    {
        var userId = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        try
        {
            await _eventService.DeleteEvent(id, userId);
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }

        return Ok();
    }

    [Authorize]
    [HttpPost("assign-user")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AssignUser([FromBody] UserEventInteractionRequest request)
    {
        var userId = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        try
        {
            await _eventService.AssignUser(request, userId);
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }

        return Ok();
    }
    
    [Authorize]
    [HttpPost("unassign-user")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UnassignUser([FromBody] UserEventInteractionRequest request)
    {
        var userId = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        try
        {
            await _eventService.UnassignUser(request, userId);
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }

        return Ok();
    }

    [Authorize]
    [HttpPost("from-task/{taskId:int}")]
    public async Task<IActionResult> PostEventFromTask([FromRoute] int taskId)
    {
        var userId = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        await _eventService.PostTaskAsEvent(taskId, userId);
        
        return Ok();
    }

    [Authorize]
    [HttpGet("event-info/{eventId:int}")]
    public async Task<IActionResult> GetEventInfo([FromRoute] int eventId)
    {
        var userId = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        return Ok(await _eventService.GetEventInfo(eventId, userId));
    }
}