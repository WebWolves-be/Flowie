using MediatR;

namespace Flowie.Features.Tasks.GetTaskById;

public record GetTaskByIdQuery(int TaskId) : IRequest<GetTaskByIdQueryResult>;