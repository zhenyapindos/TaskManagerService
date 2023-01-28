namespace StasDiplom;

public class Comment
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public Guid UserId { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public string Text { get; set; }
}