using MediatR;

namespace Flowie.Features.Tasks.CreateTask;

public record CreateTaskCommand(
    int ProjectId, 
    string Title, 
    int TypeId,
    string? Description = null,
    DateOnly? DueDate = null,
    int? AssigneeId = null,
    int? ParentTaskId = null) : IRequest<CreateTaskResponse>;