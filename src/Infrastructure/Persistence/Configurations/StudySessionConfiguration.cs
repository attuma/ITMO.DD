using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Infrastructure.Persistence.Configurations;

public class StudySessionConfiguration : IEntityTypeConfiguration<StudySession>
{
    public void Configure(EntityTypeBuilder<StudySession> builder)
    {
        builder.ToTable("study_sessions");

        builder.HasKey(ss => ss.Id);
        builder.Property(ss => ss.Id).HasColumnName("session_id");

        builder.Property(ss => ss.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ss => ss.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();

        builder.Property(ss => ss.TaskId)
            .HasColumnName("task_id");

        builder.Property(ss => ss.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(ss => ss.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(ss => ss.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ss => ss.SessionStatus)
            .HasColumnName("session_status")
            .HasMaxLength(20)
            .HasConversion(
                status => status == StudySessionStatus.Paused
                    ? "paused"
                    : status == StudySessionStatus.Completed
                        ? "completed"
                        : status == StudySessionStatus.Cancelled
                            ? "cancelled"
                            : "active",
                value => value == "paused"
                    ? StudySessionStatus.Paused
                    : value == "completed"
                        ? StudySessionStatus.Completed
                        : value == "cancelled"
                            ? StudySessionStatus.Cancelled
                            : StudySessionStatus.Active)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ss => ss.UserId)
            .HasConstraintName("fk_study_sessions_user")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(ss => ss.SubjectId)
            .HasConstraintName("fk_study_sessions_subject")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TaskItem>()
            .WithMany()
            .HasForeignKey(ss => ss.TaskId)
            .HasConstraintName("fk_study_sessions_task")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
