using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Infrastructure.Persistence.Configurations;

public class TaskProgressConfiguration : IEntityTypeConfiguration<TaskProgress>
{
    public void Configure(EntityTypeBuilder<TaskProgress> builder)
    {
        builder.ToTable("task_progress");

        builder.HasKey(tp => new { tp.TaskId, tp.UserId });
        builder.Property(tp => tp.TaskId).HasColumnName("task_id");
        builder.Property(tp => tp.UserId).HasColumnName("user_id");

        builder.Property(tp => tp.ProgressStatus)
            .HasColumnName("progress_status")
            .HasMaxLength(20)
            .HasConversion(
                status => status == TaskProgressStatus.InProgress
                    ? "in_progress"
                    : status == TaskProgressStatus.Completed
                        ? "completed"
                        : status == TaskProgressStatus.Dismissed
                            ? "dismissed"
                            : "not_started",
                value => value == "in_progress"
                    ? TaskProgressStatus.InProgress
                    : value == "completed"
                        ? TaskProgressStatus.Completed
                        : value == "dismissed"
                            ? TaskProgressStatus.Dismissed
                            : TaskProgressStatus.NotStarted)
            .IsRequired();

        builder.Property(tp => tp.StartedAt)
            .HasColumnName("started_at");

        builder.Property(tp => tp.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(tp => tp.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<TaskItem>()
            .WithMany()
            .HasForeignKey(tp => tp.TaskId)
            .HasConstraintName("fk_task_progress_task")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(tp => tp.UserId)
            .HasConstraintName("fk_task_progress_user")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
