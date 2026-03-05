using MediatR;

namespace Flowie.Api.Features.Tasks.CreateTask;

public record CreateTaskCommand(
    int SectionId,
    string Title,
    int TaskTypeId,
    DateOnly? DueDate,
    int? EmployeeId,
    string? Description = null,
    int? ParentTaskId = null) : IRequest<Unit>;
