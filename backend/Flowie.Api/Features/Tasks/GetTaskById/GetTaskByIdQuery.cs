using MediatR;

namespace Flowie.Api.Features.Tasks.GetTaskById;

public record GetTaskByIdQuery(int TaskId) : IRequest<GetTaskByIdQueryResult>;