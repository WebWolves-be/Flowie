using Flowie.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flowie.Shared.Infrastructure.Database.Configurations;

public class AuditEntryEntityConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.Details)
            .HasMaxLength(4000);
            
        builder.Property(e => e.Timestamp)
            .IsRequired();
    }
}