namespace StasDiplom.Domain;

public record Comment
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string? UserId { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public string Text { get; set; }
    public User? User { get; set; }
    public ICollection<CommentUserMention> CommentUserMentions { get; set; }
    public Project Project { get; set; }
    public Task? Task { get; set; }
}