using TaskService.Dto.Comment;

namespace TaskService.Services.Interfaces;

public interface ICommentsService
{
    public Task PostComment(CreateCommentRequest request, string userId);
    public GetCommentsResponse GetCommentsForProject(int projectId, int count, int page, string userId);
    public GetCommentsResponse GetCommentsForTask(int taskId, int count, int page, string userId);
}