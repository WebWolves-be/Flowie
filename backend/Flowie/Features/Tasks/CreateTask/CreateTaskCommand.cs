using MediatR;

namespace Flowie.Features.Tasks.CreateTask;

public record CreateTaskCommand(
    int ProjectId, 
    string Title, 
    int TaskTypeId,
    DateOnly DueDate,
    int EmployeeId,
    string? Description = null,
    int? ParentTaskId = null) : IRequest<Unit>;