using MediatR;

namespace Flowie.Features.TaskTypes.CreateTaskType;

public record CreateTaskTypeCommand(string Name) : IRequest<Unit>;