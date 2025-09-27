using Flowie.Api.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flowie.Api.Shared.Infrastructure.Database.Configurations;

public class EmployeeEntityConfiguration : BaseEntityConfiguration<Employee>
{
    public override void Configure(EntityTypeBuilder<Employee> builder)
    {
        base.Configure(builder);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);
            
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.HasIndex(e => e.Email)
            .IsUnique();
    }
}