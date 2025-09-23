using MediatR;

namespace Flowie.Features.TaskTypes.GetTaskTypes;

public record GetTaskTypesQuery : IRequest<IEnumerable<TaskTypeResponse>>;