namespace TaskService.Dto.Comment;

public class CreateCommentRequest
{
    public string Text { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
}