using Flowie.Api.Shared.Domain.Entities;
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
            
        builder.Property(e => e.Description)
            .HasMaxLength(4000);
            
        builder.Property(e => e.Company)
            .HasConversion<string>();
    }
}