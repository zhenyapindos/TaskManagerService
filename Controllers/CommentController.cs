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
using Task = StasDiplom.Domain.Task;

namespace StasDiplom.Controllers;

//[ApiController]
//[Route("api/comments/")]
public class CommentController : Controller
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public CommentController(ProjectManagerContext context, UserManager<User> userManager, IMapper mapper,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PostComment([FromBody] CreateCommentRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var project = _context.Projects
            .Include(x => x.Tasks)
            .Include(x => x.ProjectUsers)
            .ThenInclude(x => x.User)
            .Include(x => x.Comments)
            .FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null) return NotFound();

        Task? task;

        if (request.TaskId != null)
        {
            task = project.Tasks.FirstOrDefault(x => x.Id == request.TaskId);

            if (task == null) return NotFound();
        }
        else
        {
            task = null;
        }

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (resultUser == null) return Forbid();

        var usernames = project.ProjectUsers.Select(x => x.User.UserName).ToList();

        var mentionedUsernames = GetBetween(request.Text, "@")
            .Where(username => usernames.Contains(username)).ToList();

        var comment = _mapper.Map<Comment>(request) with
        {
            CreationDate = DateTime.Now,
            UserId = id,
            Project = project,
            Task = task
        };

        project.Comments.Add(comment);

        await _context.SaveChangesAsync();

        var mentionUsers = project.ProjectUsers.Where(x => mentionedUsernames.Contains(x.User.UserName));

        foreach (var projectUser in mentionUsers)
        {
            await _notificationService.UserMention(projectUser.User, comment);
        }

        //await _context.SaveChangesAsync();

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

        var project = _context.Projects
            .Include(x => x.Tasks)
            .Include(x => x.ProjectUsers)
            .ThenInclude(x => x.User)
            .Include(x => x.Comments)
            .FirstOrDefault(x => x.Id == projectId);

        if (project == null) return NotFound();

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);

        if (resultUser == null || resultUser.UserProjectRole == UserProjectRole.Kicked) return Forbid();

        if (resultUser.UserProjectRole == UserProjectRole.Invited) return Forbid();

        var response = new GetCommentsResponse();

        var usernames = project.ProjectUsers.Select(x => x.User.UserName).ToList();

        response.TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(project.Comments.Count) / count));

        response.Page = page > response.TotalPages ? response.TotalPages : page;

        var commentResponses = project.Comments
            .Skip((response.Page - 1) * count)
            .Take(count)
            .Select(commentInProject =>
            {
                return _mapper.Map<CommentResponse>(commentInProject) with
                {
                    TaggedUsernames = GetBetween(commentInProject.Text, "@")
                        .Where(username => usernames.Contains(username)).ToList()
                };
            }).ToList();

        response.Comments = commentResponses;
        response.Count = commentResponses.Count;
        response.ProjectId = projectId;

        return Ok(response);
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

        var task = _context.Tasks
            .Include(x => x.Comments)
            .Include(x => x.TaskUsers)
            .ThenInclude(x => x.User)
            .Include(x => x.Comments)
            .FirstOrDefault(x => x.Id == taskId);

        if (task == null) return NotFound();

        var resultUser = task.TaskUsers.FirstOrDefault(x => x.UserId == id);

        if (resultUser == null) return Forbid();

        var response = new GetCommentsResponse();

        var usernames = task.TaskUsers.Select(x => x.User.UserName).ToList();

        response.TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(task.Comments.Count) / count));

        response.Page = page > response.TotalPages ? response.TotalPages : page;

        var commentResponses = task.Comments
            .Skip((response.Page - 1) * count)
            .Take(count)
            .Select(commentInProject =>
            {
                return _mapper.Map<CommentResponse>(commentInProject) with
                {
                    TaggedUsernames = GetBetween(commentInProject.Text, "@")
                        .Where(username => usernames.Contains(username)).ToList()
                };
            }).ToList();

        response.Comments = commentResponses;
        response.Count = commentResponses.Count;
        response.TaskId = taskId;

        return Ok(response);
    }
    
    public IEnumerable<string> GetBetween(string strSource, string strStart)
    {
        var start = strSource.IndexOf(strStart);

        while (start != -1)
        {
            var username = string.Concat(strSource[(start + 1)..]
                .TakeWhile(x => _userManager.Options.User.AllowedUserNameCharacters
                    .Contains(x)));

            start = start + username.Length < strSource.Length
                ? strSource.IndexOf(strStart, start + username.Length)
                : -1;

            yield return username;
        }

        yield break;
    }
}