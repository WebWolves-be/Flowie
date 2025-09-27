namespace Flowie.Api.Shared.Infrastructure.Exceptions;

public class EntityNotFoundException : Exception
{
    public string EntityName { get; }
    
    public int EntityId { get; }

    public EntityNotFoundException() : base("Entity not found.")
    {
    }

    public EntityNotFoundException(string message) : base(message)
    {
    }

    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public EntityNotFoundException(string entityName, int entityId) 
        : base($"{entityName} with ID {entityId} not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
