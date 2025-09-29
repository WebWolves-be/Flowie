using Flowie.Api.Shared.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flowie.Api.Shared.Infrastructure.Database.Configurations.Identity;

public class RefreshTokenConfiguration : BaseEntityConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(rt => rt.UserId)
            .IsRequired();
        
        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();
        
        builder.Property(rt => rt.CreatedAt)
            .IsRequired();
        
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(rt => rt.Token)
            .IsUnique();
        
        builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked });
    }
}