using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("subject_id");

        builder.Property(s => s.SubjectName)
            .HasColumnName("subject_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasColumnType("TEXT");

        builder.Property(s => s.OwnerUserId)
            .HasColumnName("owner_user_id");

        builder.Property(s => s.OwnerGroupId)
            .HasColumnName("owner_group_id");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.OwnerUserId)
            .HasConstraintName("fk_subjects_owner_user")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(s => s.OwnerGroupId)
            .HasConstraintName("fk_subjects_owner_group")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.IsArchived)
            .HasColumnName("is_archived")
            .IsRequired();

        builder.Property(s => s.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
