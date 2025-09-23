namespace Flowie.Shared.Domain.Exceptions;

public class ParentTaskNotFoundException : DomainException
{
    public int TaskId { get; }
    public int? ProjectId { get; }

    public ParentTaskNotFoundException() : base("Parent task not found.")
    {
    }

    public ParentTaskNotFoundException(string message) : base(message)
    {
    }

    public ParentTaskNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ParentTaskNotFoundException(int taskId) 
        : base($"Parent task with ID {taskId} not found.")
    {
        TaskId = taskId;
    }

    public ParentTaskNotFoundException(int taskId, int projectId) 
        : base($"Parent task with ID {taskId} not found in project with ID {projectId}.")
    {
        TaskId = taskId;
        ProjectId = projectId;
    }
}