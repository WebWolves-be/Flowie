using MediatR;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

public record DeleteTaskTypeCommand(int Id) : IRequest<DeleteTaskTypeResponse>;