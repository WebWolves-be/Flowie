using MediatR;

namespace Flowie.Api.Features.Tasks.DeleteTask;

public record DeleteTaskCommand(int TaskId) : IRequest<Unit>;