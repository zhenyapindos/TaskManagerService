using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto.Comment;
using StasDiplom.Enum;
using StasDiplom.Utility;
using Task = StasDiplom.Domain.Task;

namespace StasDiplom.Controllers;

public class CommentController : Controller
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager; 
    private readonly IMapper _mapper;
    
    public CommentController(ProjectManagerContext context, UserManager<User> userManager, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }

    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PostComment(CreateCommentRequest request)
    {
        var id = User.Claims.First(x => x.Type == MyClaims.Id).Value ?? throw new ArgumentException();

        var project = _context.Projects
            .Include(x=> x.Tasks)
            .Include(x => x.ProjectUsers)
            .FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null) return NotFound();
        
        var task = project.Tasks.FirstOrDefault(x => x.Id == request.TaskId);
        
        if (task == null) return NotFound();

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == id);
            
        if (resultUser == null) return Forbid();

        var comment = _mapper.Map<Comment>(request) with
        {
            CreationDate = DateTime.Now,
            UserId = id,
            Project = project,
            Task = task
        };
        
        project.Comments.Add(comment);
        
        await _context.SaveChangesAsync();
        
        return Ok();
    }

    [Authorize]
    [HttpGet("/for-project/{projectId:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCommentsForProject([FromRoute] int projectId, [FromQuery] int count = 100, [FromQuery] int page = 0)
    {
        return Ok();
    }
}