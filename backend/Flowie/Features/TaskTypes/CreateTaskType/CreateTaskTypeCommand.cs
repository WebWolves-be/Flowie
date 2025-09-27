using MediatR;

namespace Flowie.Features.TaskTypes.CreateTaskType;

public record CreateTaskTypeCommand(string Name, string? Description = null, string? Color = null) 
    : IRequest<CreateTaskTypeCommandResult>;