using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto.Comment;
using StasDiplom.Dto.Project;
using StasDiplom.Enum;
using StasDiplom.Services;
using StasDiplom.Services.Interfaces;
using StasDiplom.Utility;
using TaskService.Services;
using TaskService.Services.Interfaces;
using Task = StasDiplom.Domain.Task;

namespace StasDiplom.Controllers;

[ApiController]
[Route("api/comments/")]
public class CommentController : Controller
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly ICommentsService _commentsService;
    public CommentController(ProjectManagerContext context, UserManager<User> userManager, IMapper mapper,
        INotificationService notificationService, ICommentsService commentsService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _notificationService = notificationService;
        _commentsService = commentsService;
    }

    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PostComment([FromBody] CreateCommentRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        await _commentsService.PostComment(request, id);
        
        return Ok();
    }

    [Authorize]
    [HttpGet("for-project/{projectId:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCommentsForProject([FromRoute] int projectId, [FromQuery] int count = 100,
        [FromQuery] int page = 1)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();
        
        return Ok(_commentsService.GetCommentsForProject(projectId, count, page, id));
    }

    [Authorize]
    [HttpGet("for-task/{taskId:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCommentsForTask([FromRoute] int taskId, [FromQuery] int count = 100,
        [FromQuery] int page = 1)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        return Ok( _commentsService.GetCommentsForTask(taskId, count, page, id));
    }
}