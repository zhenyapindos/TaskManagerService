using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskService.Domain;
using TaskService.Extensions;
using Task = TaskService.Domain.Task;

namespace TaskService.Context;

public class ProjectManagerContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventUser> EventUsers { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskUser> TaskUsers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectUser> ProjectUsers { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public ProjectManagerContext(DbContextOptions<ProjectManagerContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .CommentToProject()
            .CommentToTask()
            .TaskToProject()
            .ProjectToNotification()
            .EventToCalendar()
            .UserToComment()
            .UserToNotification()
            .UserToEvent()
            .UserToTask()
            .UserToProject()
            .ProjectToCalendar()
            .CommentToNotification()
            .EventToEventUsers()
            .EventToTask();
    }
}