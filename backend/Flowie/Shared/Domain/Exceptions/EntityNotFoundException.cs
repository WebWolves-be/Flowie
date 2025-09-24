namespace Flowie.Shared.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public EntityNotFoundException() : base("Entity not found.")
    {
    }

    public EntityNotFoundException(string message) : base(message)
    {
    }

    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public EntityNotFoundException(string entityName, object entityId) 
        : base($"{entityName} with ID {entityId} not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}