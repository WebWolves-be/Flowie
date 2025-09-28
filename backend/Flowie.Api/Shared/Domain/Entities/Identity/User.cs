using Flowie.Api.Shared.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Flowie.Api.Shared.Domain.Entities.Identity;

public class User : IdentityUser, IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }

    public Employee Employee { get; set; }
}