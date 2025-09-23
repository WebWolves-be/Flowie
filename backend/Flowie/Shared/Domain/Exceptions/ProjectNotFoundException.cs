namespace Flowie.Shared.Domain.Exceptions;

public class ProjectNotFoundException : DomainException
{
    public int ProjectId { get; }

    public ProjectNotFoundException() : base("Project not found.")
    {
    }

    public ProjectNotFoundException(string message) : base(message)
    {
    }

    public ProjectNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ProjectNotFoundException(int projectId) 
        : base($"Project with ID {projectId} not found.")
    {
        ProjectId = projectId;
    }
}