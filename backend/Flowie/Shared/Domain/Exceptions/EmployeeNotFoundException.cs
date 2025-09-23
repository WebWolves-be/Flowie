namespace Flowie.Shared.Domain.Exceptions;

public class EmployeeNotFoundException : DomainException
{
    public int EmployeeId { get; }

    public EmployeeNotFoundException() : base("Employee not found.")
    {
    }

    public EmployeeNotFoundException(string message) : base(message)
    {
    }

    public EmployeeNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public EmployeeNotFoundException(int employeeId) 
        : base($"Employee with ID {employeeId} not found.")
    {
        EmployeeId = employeeId;
    }
}