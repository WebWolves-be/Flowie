using MediatR;

namespace Flowie.Api.Features.TaskTypes.CreateTaskType;

public record CreateTaskTypeCommand(string Name) : IRequest<Unit>;