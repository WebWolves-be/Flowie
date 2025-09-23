namespace Flowie.Shared.Domain.Exceptions;

public class TaskTypeNotFoundException : DomainException
{
    public int TaskTypeId { get; }

    public TaskTypeNotFoundException() : base("Task type not found.")
    {
    }

    public TaskTypeNotFoundException(string message) : base(message)
    {
    }

    public TaskTypeNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TaskTypeNotFoundException(int taskTypeId) 
        : base($"Task type with ID {taskTypeId} not found.")
    {
        TaskTypeId = taskTypeId;
    }
}