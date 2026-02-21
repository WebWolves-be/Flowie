using MediatR;

namespace Flowie.Api.Features.Tasks.ReorderTasks;

public record ReorderTasksCommand(IReadOnlyList<TaskOrderItem> Items) : IRequest<Unit>;

public record TaskOrderItem(int TaskId, int DisplayOrder);
