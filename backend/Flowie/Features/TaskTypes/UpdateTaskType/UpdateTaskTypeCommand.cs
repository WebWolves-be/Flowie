using MediatR;

namespace Flowie.Features.TaskTypes.UpdateTaskType;

public record UpdateTaskTypeCommand(int Id, string? Name = null, string? Description = null, string? Color = null) 
    : IRequest<UpdateTaskTypeCommandResult>;