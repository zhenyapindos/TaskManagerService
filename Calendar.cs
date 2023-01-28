namespace StasDiplom;

public class Calendar
{
    public int Id { get; set; }
    public int? ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public string Title { get; set; }
}