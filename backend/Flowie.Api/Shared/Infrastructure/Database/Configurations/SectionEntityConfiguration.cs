using Flowie.Api.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flowie.Api.Shared.Infrastructure.Database.Configurations;

public class SectionEntityConfiguration : BaseEntityConfiguration<Section>
{
    public override void Configure(EntityTypeBuilder<Section> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.HasIndex(e => new { e.ProjectId, e.Title })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(s => s.Project)
            .WithMany(p => p.Sections)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
