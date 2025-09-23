namespace Flowie.Shared.Domain.Exceptions;

public class ParentTaskProjectMismatchException : DomainException
{
    public int TaskId { get; }
    public int ProjectId { get; }

    public ParentTaskProjectMismatchException() : base("Parent task does not belong to the specified project.")
    {
    }

    public ParentTaskProjectMismatchException(string message) : base(message)
    {
    }

    public ParentTaskProjectMismatchException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ParentTaskProjectMismatchException(int taskId, int projectId) 
        : base($"Parent task with ID {taskId} does not belong to project with ID {projectId}.")
    {
        TaskId = taskId;
        ProjectId = projectId;
    }
}