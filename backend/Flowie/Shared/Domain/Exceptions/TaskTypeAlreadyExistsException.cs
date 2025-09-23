namespace Flowie.Shared.Domain.Exceptions;

public class TaskTypeAlreadyExistsException : DomainException
{
    public string? TaskTypeName { get; }

    public TaskTypeAlreadyExistsException() : base("Task type with the same name already exists.")
    {
    }

    public TaskTypeAlreadyExistsException(string message) : base(message)
    {
    }

    public TaskTypeAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TaskTypeAlreadyExistsException(string taskTypeName, bool isTaskTypeName) 
        : base($"Task type with name '{taskTypeName}' already exists.")
    {
        if (isTaskTypeName)
        {
            TaskTypeName = taskTypeName;
        }
    }
}