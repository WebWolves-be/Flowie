using MediatR;

namespace Flowie.Api.Features.TaskTypes.DeleteTaskType;

public record DeleteTaskTypeCommand(int Id) : IRequest<Unit>;