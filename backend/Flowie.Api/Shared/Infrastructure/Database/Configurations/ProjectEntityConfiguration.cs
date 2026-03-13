using Flowie.Api.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flowie.Api.Shared.Infrastructure.Database.Configurations;

public class ProjectEntityConfiguration : BaseEntityConfiguration<Project>
{
    public override void Configure(EntityTypeBuilder<Project> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(e => e.Title)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(e => e.Code)
            .HasMaxLength(5);

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.Company)
            .HasConversion<string>();

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}