namespace StasDiplom.Dto.Comment;
using Comment = Domain.Comment;
public record GetCommentsResponse
{
    public IEnumerable<CommentResponse> Comments { get; set; }
    public int Count { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    //public string[] TaggedUsernames { get; set; }
}