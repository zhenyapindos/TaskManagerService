namespace StasDiplom.Domain;

public record CommentUserMention
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public int NotificationId { get; set; }
    
    public Comment Comment { get; set; }
}