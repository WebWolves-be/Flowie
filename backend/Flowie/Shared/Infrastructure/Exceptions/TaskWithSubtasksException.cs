namespace Flowie.Shared.Infrastructure.Exceptions;

public class TaskWithSubtasksException : DomainException
{
    public int TaskId { get; }

    public TaskWithSubtasksException() : base("Cannot perform operation on a task with subtasks.")
    {
    }

    public TaskWithSubtasksException(string message) : base(message)
    {
    }

    public TaskWithSubtasksException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TaskWithSubtasksException(int taskId) 
        : base($"Cannot delete task with ID {taskId} because it has subtasks. Delete the subtasks first.")
    {
        TaskId = taskId;
    }
}
