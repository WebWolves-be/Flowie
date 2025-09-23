namespace Flowie.Shared.Domain.Exceptions;

public class TaskTypeInUseException : DomainException
{
    public int TaskTypeId { get; }

    public TaskTypeInUseException() : base("Task type is in use by existing tasks and cannot be deleted.")
    {
    }

    public TaskTypeInUseException(string message) : base(message)
    {
    }

    public TaskTypeInUseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TaskTypeInUseException(int taskTypeId) 
        : base($"Task type with ID {taskTypeId} is in use by existing tasks and cannot be deleted.")
    {
        TaskTypeId = taskTypeId;
    }
}