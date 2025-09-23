using MediatR;

namespace Flowie.Features.Tasks.UpdateTask;

public record UpdateTaskCommand(
    int ProjectId,
    int TaskId,
    string? Title = null,
    string? Description = null,
    int? TypeId = null,
    DateOnly? Deadline = null,
    int? AssigneeId = null
) : IRequest<bool>;