using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StasDiplom.Domain;
using StasDiplom.Services;
using StasDiplom.Utility;

namespace StasDiplom.Controllers;

[ApiController]
[Route("api/notification/")]
public class NotificationController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly INotificationService _notificationService;

    public NotificationController(UserManager<User> userManager, INotificationService notificationService)
    {
        _userManager = userManager;
        _notificationService = notificationService;
    }

    [Authorize]
    [HttpPost("new")]
    [ProducesResponseType(401)]
    public async Task<IActionResult> IsUnreadNotifications()
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var user = _userManager.FindByIdAsync(id).Result;
        
        return Ok(_notificationService.IsUnreadNotifications(user));
    }
    
    [Authorize]
    [HttpPost("read")]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MarkAsRead([FromQuery] int id)
    {
        var userId = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var user = _userManager.FindByIdAsync(userId).Result;

        _notificationService.MarkAsRead(user, id);
        
        return Ok();
    }
    
    [Authorize]
    [HttpGet("unread")]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UnreadNotifications()
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var user = _userManager.FindByIdAsync(id).Result;

        return Ok(await _notificationService.GetUnreadNotifications(user));
    }

    [Authorize]
    [HttpGet("")]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAllNotificationsInfo()
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var user = _userManager.FindByIdAsync(id).Result;
        
        return Ok(await _notificationService.GetAllNotificationsInfo(user));
    }
}