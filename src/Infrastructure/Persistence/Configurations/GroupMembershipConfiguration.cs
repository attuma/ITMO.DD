using StudentTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Enums;


namespace StudentTracker.Infrastructure.Persistence.Configurations;

public class GroupMembershipConfiguration : IEntityTypeConfiguration<GroupMembership>
{
    public void Configure(EntityTypeBuilder<GroupMembership> builder)
    {
        builder.ToTable("group_memberships");

        builder.HasKey(gm => new { gm.GroupId, gm.UserId });
        builder.Property(gm => gm.UserId).HasColumnName("user_id");
        builder.Property(gm => gm.GroupId).HasColumnName("group_id");

        builder.Property(gm => gm.GroupRole)
            .HasColumnName("group_role")
            .HasMaxLength(20)
            .HasConversion(
            role => role == GroupRole.GroupOwner ? "group_owner" : "member",
            value => value == "group_owner" ? GroupRole.GroupOwner : GroupRole.Member)
            .IsRequired();

        builder.Property(gm => gm.JoinedAt)
            .HasColumnName("joined_at")
            . IsRequired();


        builder.Property(gm => gm.LeftAt)
            .HasColumnName("left_at");

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(gm => gm.GroupId)
            .HasConstraintName("fk_group_memberships_group")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(gm => gm.UserId)
            .HasConstraintName("fk_group_memberships_user")
            .OnDelete(DeleteBehavior.Restrict);


    }
}
