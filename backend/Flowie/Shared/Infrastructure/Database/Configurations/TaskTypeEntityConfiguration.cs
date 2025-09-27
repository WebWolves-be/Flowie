using Flowie.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flowie.Shared.Infrastructure.Database.Configurations;

public class TaskTypeEntityConfiguration : BaseEntityConfiguration<TaskType>
{
    public override void Configure(EntityTypeBuilder<TaskType> builder)
    {
        base.Configure(builder);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}