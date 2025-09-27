using MediatR;

namespace Flowie.Api.Features.TaskTypes.GetTaskTypes;

public record GetTaskTypesQuery : IRequest<IEnumerable<GetTaskTypesQueryResult>>;