using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities_Task = Flowie.Api.Shared.Domain.Entities.Task;
using Task = Flowie.Api.Shared.Domain.Entities.Task;

namespace Flowie.Api.Shared.Infrastructure.Database.Configurations;

public class TaskEntityConfiguration : BaseEntityConfiguration<Entities_Task>
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
            .IsRequired();
        
        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.Subtasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(t => t.ParentTaskId);

        builder.HasOne(t => t.TaskType)
            .WithMany()
            .HasForeignKey(t => t.TaskTypeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(t => t.Employee)
            .WithMany(e => e.AssignedTasks)
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }
}