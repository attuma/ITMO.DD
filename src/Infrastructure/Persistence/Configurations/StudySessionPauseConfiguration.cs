using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Persistence.Configurations;

public class StudySessionPauseConfiguration : IEntityTypeConfiguration<StudySessionPause>
{
    public void Configure(EntityTypeBuilder<StudySessionPause> builder)
    {
        builder.ToTable("study_session_pauses");

        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Id).HasColumnName("pause_id");

        builder.Property(sp => sp.SessionId)
            .HasColumnName("session_id")
            .IsRequired();

        builder.Property(sp => sp.PausedAt)
            .HasColumnName("paused_at")
            .IsRequired();

        builder.Property(sp => sp.ResumedAt)
            .HasColumnName("resumed_at");

        builder.HasOne<StudySession>()
            .WithMany()
            .HasForeignKey(sp => sp.SessionId)
            .HasConstraintName("fk_study_session_pauses_session")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
