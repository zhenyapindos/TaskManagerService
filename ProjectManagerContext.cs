﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StasDiplom;

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
    public DbSet<CommentUserMention> CommentUserMentions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    
    public ProjectManagerContext(DbContextOptions<ProjectManagerContext> options) : base(options)
    { }
}