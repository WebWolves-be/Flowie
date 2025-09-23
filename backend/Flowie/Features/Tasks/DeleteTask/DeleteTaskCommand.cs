using MediatR;

namespace Flowie.Features.Tasks.DeleteTask;

public record DeleteTaskCommand(int ProjectId, int TaskId) : IRequest<bool>;