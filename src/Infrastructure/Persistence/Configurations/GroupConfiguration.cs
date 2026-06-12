using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentTracker.Domain.Entities;

namespace StudentTracker.Infrastructure.Persistence.Configurations;

public class GroupConfiguration: IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");
    }
}

