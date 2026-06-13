using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("task_items");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("task_id");

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasColumnType("TEXT");

        builder.Property(t => t.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(t => t.SubjectId)
            .HasConstraintName("fk_task_items_subject")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.OwnerUserId)
            .HasColumnName("owner_user_id");

        builder.Property(t => t.OwnerGroupId)
            .HasColumnName("owner_group_id");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.OwnerUserId)
            .HasConstraintName("fk_task_items_owner_user")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(t => t.OwnerGroupId)
            .HasConstraintName("fk_task_items_owner_group")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.DeadlineAt)
            .HasColumnName("deadline_at");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.TaskLink)
            .HasColumnName("task_link")
            .HasMaxLength(500);

        builder.Property(t => t.MaxPoints)
            .HasColumnName("max_points");

        builder.Property(t => t.IsArchived)
            .HasColumnName("is_archived")
            .IsRequired();
    }
}
