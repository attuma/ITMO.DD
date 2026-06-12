using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Persistence.Configurations;

public class GroupConfiguration: IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("group_id");

        builder.Property(g => g.GroupName)
            .HasColumnName("group_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(g => g.Description)
            .HasColumnName("description")
            .HasColumnType("TEXT");
        
        builder.Property(g => g.OwnerUserId)
            .HasColumnName("owner_user_id")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(g => g.OwnerUserId)
            .HasConstraintName("fk_groups_owner_user")
            .OnDelete(DeleteBehavior.Restrict);


        builder.Property(g => g.IsArchived)
            .HasColumnName("is_archived")
            .IsRequired();

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}

