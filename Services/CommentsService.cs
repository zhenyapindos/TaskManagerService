using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Domain;
using TaskService.Dto.Comment;
using TaskService.Enum;
using TaskService.Services.Interfaces;
using Task = System.Threading.Tasks.Task;
using DomainTask = TaskService.Domain.Task;

namespace TaskService.Services;

public class CommentsService : ICommentsService
{
    private readonly ProjectManagerContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public CommentsService(INotificationService notificationService, ProjectManagerContext context,
         UserManager<User> userManager, IMapper mapper)
    {
        _notificationService = notificationService;
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task PostComment(CreateCommentRequest request, string userId)
    {
        var project = _context.Projects
            .Include(x => x.Tasks)
            .Include(x => x.ProjectUsers)
            .ThenInclude(x => x.User)
            .Include(x => x.Comments)
            .FirstOrDefault(x => x.Id == request.ProjectId);

        if (project == null)
        {
            throw new ArgumentNullException();
        }

        DomainTask? task;

        if (request.TaskId != null)
        {
            task = project.Tasks.FirstOrDefault(x => x.Id == request.TaskId);

            if (project == null)
            {
                throw new ArgumentNullException();
            }
        }
        else
        {
            task = null;
        }

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == userId);

        if (resultUser == null)
        {
            throw new InvalidOperationException();
        }

        var usernames = project.ProjectUsers.Select(x => x.User.UserName).ToList();

        var mentionedUsernames = GetBetween(request.Text, "@")
            .Where(username => usernames.Contains(username)).ToList();

        var comment = _mapper.Map<Comment>(request) with
        {
            CreationDate = DateTime.Now,
            UserId = userId,
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
    }

    public GetCommentsResponse GetCommentsForProject(int projectId, int count, int page, string userId)
    {
        var project = _context.Projects
            .Include(x => x.Tasks)
            .Include(x => x.ProjectUsers)
            .ThenInclude(x => x.User)
            .Include(x => x.Comments)
            .FirstOrDefault(x => x.Id == projectId);

        if (project == null)
        {
            throw new ArgumentNullException();
        }

        var resultUser = project.ProjectUsers.FirstOrDefault(x => x.UserId == userId);

        if (resultUser == null || resultUser.UserProjectRole == UserProjectRole.Kicked)
        {
            throw new InvalidOperationException();
        }

        if (resultUser.UserProjectRole == UserProjectRole.Invited)
        {
            throw new InvalidOperationException();
        }

        var response = CreateCommentList(
            project.Comments.Where(x=> x.Task == null).ToList(),
            count,
            page,
            project.ProjectUsers.Select(x => x.User.UserName).ToList());

        response.ProjectId = projectId;

        return response;
    }

    public GetCommentsResponse GetCommentsForTask(int taskId, int count, int page, string userId)
    {
        var task = _context.Tasks
            .Include(x => x.Comments)
            .Include(x => x.TaskUsers)
            .ThenInclude(x => x.User)
            .Include(x => x.Comments)
            .FirstOrDefault(x => x.Id == taskId);

        if (task == null)
        {
            throw new ArgumentNullException();
        }
        
        var response = CreateCommentList(
            task.Comments.ToList(),
            count,
            page,
            task.TaskUsers.Select(x => x.User.UserName).ToList());

        response.TaskId = taskId;

        return response;
    }

    private IEnumerable<string> GetBetween(string strSource, string strStart)
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

    private GetCommentsResponse CreateCommentList(ICollection<Comment> comments, int count, int page,
        List<string> usernames)
    {
        var response = new GetCommentsResponse
        {
            TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(comments.Count) / count))
        };

        response.Page = page > response.TotalPages ? response.TotalPages : page;

        List<CommentResponse> commentResponses;

        if (count > comments.Count)
        {
            commentResponses = comments.Select(commentInProject =>
            {
                return _mapper.Map<CommentResponse>(commentInProject) with
                {
                    TaggedUsernames = GetBetween(commentInProject.Text, "@")
                        .Where(username => usernames.Contains(username)).ToList()
                };
            }).ToList();
            
            response.Comments = commentResponses;
            response.Count = commentResponses.Count;

            return response;
        }

        commentResponses = comments
            .OrderBy(x => x.CreationDate)
            .SkipLast((response.Page -1) * count)
            .TakeLast(count)
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

        return response;
    }
}