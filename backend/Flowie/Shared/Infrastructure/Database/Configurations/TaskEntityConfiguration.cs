using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Flowie.Shared.Domain.Entities;
using Task = Flowie.Shared.Domain.Entities.Task;

namespace Flowie.Shared.Infrastructure.Database.Configurations;

public class TaskEntityConfiguration : BaseEntityConfiguration<Task>
{
    public override void Configure(EntityTypeBuilder<Task> builder)
    {
        base.Configure(builder);
        
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(e => e.Description)
            .HasMaxLength(4000);
            
        builder.Property(e => e.Status)
            .HasConversion<string>();
            
        builder.Property(e => e.DueDate)
            .IsRequired(false);
        
        // Relationships
        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.Subtasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
            
        builder.HasOne(t => t.TaskType)
            .WithMany(tt => tt.Tasks)
            .HasForeignKey(t => t.TypeId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(t => t.Employee)
            .WithMany(e => e.AssignedTasks)
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}