using Microsoft.EntityFrameworkCore;
using StudentTracker.Domain.Entities;
using StudentTracker.Infrastructure.Persistence.Configurations;

namespace StudentTracker.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<TaskProgress> TaskProgress => Set<TaskProgress>();
    public DbSet<StudySession> StudySessions => Set<StudySession>();
    public DbSet<StudySessionPause> StudySessionPauses => Set<StudySessionPause>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GroupConfiguration());
        modelBuilder.ApplyConfiguration(new GroupMembershipConfiguration());
        modelBuilder.ApplyConfiguration(new SubjectConfiguration());
        modelBuilder.ApplyConfiguration(new TaskItemConfiguration());
        modelBuilder.ApplyConfiguration(new TaskProgressConfiguration());
        modelBuilder.ApplyConfiguration(new StudySessionConfiguration());
        modelBuilder.ApplyConfiguration(new StudySessionPauseConfiguration());
    }
}
