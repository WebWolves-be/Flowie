using Flowie.Features.Tasks.GetTasks;
using MediatR;

namespace Flowie.Features.Tasks.GetTaskById;

public record GetTaskByIdQuery(int ProjectId, int TaskId) : IRequest<TaskDto>;