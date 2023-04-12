using Microsoft.EntityFrameworkCore;
using StasDiplom.Domain;
using StasDiplom.Dto.Event;
using TaskService.Enum;
using Task = StasDiplom.Domain.Task;

namespace StasDiplom.Extensions;

public static class ModelBuilderExtension
{
    /*public static ModelBuilder EventToTask(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>()
            .HasOne(t => t.Task)
            .WithMany(t => t.Events)
            .HasForeignKey(e => e.Task)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Task>()
            .HasMany(t => t.Events)
            .WithOne(t => t.Task)
            .OnDelete(DeleteBehavior.NoAction);

        return modelBuilder;
    }*/
    public static ModelBuilder CommentToTask(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>()
            .HasOne(t => t.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(t => t.TaskId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Task>()
            .HasMany(t => t.Comments)
            .WithOne(t => t.Task)
            .OnDelete(DeleteBehavior.Cascade);

        return modelBuilder;
    }

    public static ModelBuilder CommentToProject(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>()
            .HasOne(t => t.Project)
            .WithMany(t => t.Comments)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Project>()
            .HasMany(t => t.Comments)
            .WithOne(t => t.Project)
            .OnDelete(DeleteBehavior.NoAction);

        return modelBuilder;
    }

    public static ModelBuilder TaskToProject(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>()
            .HasOne(t => t.Project)
            .WithMany(t => t.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Project>()
            .HasMany(t => t.Tasks)
            .WithOne(t => t.Project)
            .OnDelete(DeleteBehavior.Cascade);

        return modelBuilder;
    }

    public static ModelBuilder ProjectToNotification(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>()
            .HasOne(t => t.Project)
            .WithMany(t => t.Notifications)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Project>()
            .HasMany(n => n.Notifications)
            .WithOne(n => n.Project)
            .OnDelete(DeleteBehavior.NoAction);

        return modelBuilder;
    }

    public static ModelBuilder TaskToNotification(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>()
            .HasOne(t => t.Task)
            .WithMany(t => t.Notifications)
            .HasForeignKey(t => t.TaskId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Task>()
            .HasMany(n => n.Notifications)
            .WithOne(n => n.Task)
            .OnDelete(DeleteBehavior.Cascade);

        return modelBuilder;
    }

    public static ModelBuilder EventToCalendar(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>()
            .HasOne(c => c.Calendar)
            .WithMany(c => c.Events)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Calendar>()
            .HasMany(e => e.Events)
            .WithOne(e => e.Calendar)
            .HasForeignKey(e => e.CalendarId)
            .OnDelete(DeleteBehavior.Cascade);

        return modelBuilder;
    }

    public static ModelBuilder UserToComment(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(c => c.Comments)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Comment>()
            .HasOne(u => u.User)
            .WithMany(u => u.Comments)
            .OnDelete(DeleteBehavior.NoAction);

        return modelBuilder;
    }

    public static ModelBuilder UserToNotification(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(n => n.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(c => c.User)
            .WithMany(c => c.Notifications)
            .OnDelete(DeleteBehavior.NoAction);

        return modelBuilder;
    }

    public static ModelBuilder UserToEvent(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(x => x.Events)
            .WithMany(x => x.Users)
            .UsingEntity<EventUser>(
                j => j
                    .HasOne(p => p.Event)
                    .WithMany(p => p.EventUsers)
                    .HasForeignKey(p => p.EventId)
                    .OnDelete(DeleteBehavior.NoAction),
                j => j
                    .HasOne(u => u.User)
                    .WithMany(u => u.EventUsers)
                    .HasForeignKey(u => u.UserId)
                    .OnDelete(DeleteBehavior.NoAction),
                j => j.HasKey(l => new {l.EventId, l.UserId}));

        return modelBuilder;
    }

    public static ModelBuilder UserToTask(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(x => x.Tasks)
            .WithMany(x => x.Users)
            .UsingEntity<TaskUser>(
                j => j
                    .HasOne(p => p.Task)
                    .WithMany(p => p.TaskUsers)
                    .HasForeignKey(p => p.TaskId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(u => u.User)
                    .WithMany(u => u.TaskUsers)
                    .HasForeignKey(u => u.UserId)
                    .OnDelete(DeleteBehavior.NoAction),
                j => j.HasKey(l => new {l.TaskId, l.UserId}));

        return modelBuilder;
    }

    public static ModelBuilder UserToProject(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(x => x.Projects)
            .WithMany(x => x.Users)
            .UsingEntity<ProjectUser>(
                j => j
                    .HasOne(p => p.Project)
                    .WithMany(p => p.ProjectUsers)
                    .HasForeignKey(p => p.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(u => u.User)
                    .WithMany(u => u.ProjectUsers)
                    .HasForeignKey(u => u.UserId)
                    .OnDelete(DeleteBehavior.NoAction),
                j => j.HasKey(l => new {l.ProjectId, l.UserId}));
        
        return modelBuilder;
    }

    public static ModelBuilder ProjectToCalendar(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .HasOne(c => c.Calendar)
            .WithOne(c => c.Project)
            .HasForeignKey<Calendar>(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Calendar>()
            .HasOne(c => c.Project)
            .WithOne(c => c.Calendar)
            .HasForeignKey<Project>(c => c.CalendarId)
            .OnDelete(DeleteBehavior.NoAction);

        return modelBuilder;
    }
    
    public static ModelBuilder CommentToNotification(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>()
            .HasOne(c => c.Comment)
            .WithMany()
            .HasForeignKey(c => c.CommentId)
            .OnDelete(DeleteBehavior.NoAction);

        return modelBuilder;
    }

    public static ModelBuilder EventToEventUsers(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>()
            .HasMany(e => e.EventUsers)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<EventUser>()
            .HasOne(e => e.Event)
            .WithMany(e => e.EventUsers)
            .OnDelete(DeleteBehavior.Cascade);
            
        return modelBuilder;
    }
}