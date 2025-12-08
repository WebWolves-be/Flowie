using Flowie.Api.Shared.Domain.Entities.Identity;

namespace Flowie.Api.Shared.Domain.Entities;

public class Employee : BaseEntity
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }
    
    public required string UserId { get; set; }

    public bool Active { get; set; } = true;
    
    public User? User { get; set; }
    
    public ICollection<Task> AssignedTasks { get; } = [];
}