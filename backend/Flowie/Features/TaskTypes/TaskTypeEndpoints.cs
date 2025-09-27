using Flowie.Features.TaskTypes.CreateTaskType;
using Flowie.Features.TaskTypes.DeleteTaskType;
using Flowie.Features.TaskTypes.GetTaskTypes;
using Flowie.Features.TaskTypes.UpdateTaskType;

namespace Flowie.Features.TaskTypes;

internal static class TaskTypeEndpoints
{
    public static void MapTaskTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var taskTypes = app.MapGroup("/api/task-types")
            .WithOpenApi()
            .WithTags("TaskTypes");

        GetTaskTypesEndpoint.Map(taskTypes);
        CreateTaskTypeEndpoint.Map(taskTypes);
        UpdateTaskTypeEndpoint.Map(taskTypes);
        DeleteTaskTypeEndpoint.Map(taskTypes);
    }
}