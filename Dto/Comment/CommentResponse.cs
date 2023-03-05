using StasDiplom.Dto.Project;

namespace StasDiplom.Dto.Comment;

public record CommentResponse
{
    public int Id { get; set; }
    public string Text { get; set; }
    public DateTime CreationDate { get; set; }
    public UserShortInfo UserInfo { get; set; }
    public List<string> TaggedUsernames { get; set; }
}