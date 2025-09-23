namespace Flowie.Shared.Domain.Exceptions;

public class TaskNotFoundException : DomainException
{
    public int TaskId { get; }
    public int ProjectId { get; }

    public TaskNotFoundException() : base("Task not found.")
    {
    }

    public TaskNotFoundException(string message) : base(message)
    {
    }

    public TaskNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TaskNotFoundException(int taskId, int projectId) 
        : base($"Task with ID {taskId} not found in project with ID {projectId}.")
    {
        TaskId = taskId;
        ProjectId = projectId;
    }
}