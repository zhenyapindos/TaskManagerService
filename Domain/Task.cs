﻿namespace StasDiplom.Domain;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Status { get; set; }
    public DateTime CreationTime { get; set; }
    public double? DurationTime { get; set; }
    public int? ParentTaskId { get; set; }
    public int? PreviousTaskId { get; set; }
    public DateTime? StartDate { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public ICollection<TaskUser> TaskUsers { get; set; }
    public ICollection<User> Users { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Event> Events { get; set; }
}