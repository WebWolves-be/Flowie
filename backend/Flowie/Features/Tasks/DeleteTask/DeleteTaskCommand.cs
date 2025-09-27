using MediatR;

namespace Flowie.Features.Tasks.DeleteTask;

public record DeleteTaskCommand(int TaskId) : IRequest<Unit>;