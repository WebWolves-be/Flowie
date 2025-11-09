using Flowie.Api.Features.TaskTypes.CreateTaskType;
using Flowie.Api.Features.TaskTypes.DeleteTaskType;
using Flowie.Api.Features.TaskTypes.GetTaskTypes;

namespace Flowie.Api.Features.TaskTypes;

internal static class TaskTypeEndpoints
{
    public static void MapTaskTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var taskTypes = app
            .MapGroup("/api/task-types")
            //.RequireAuthorization()
            .WithOpenApi()
            .WithTags("TaskTypes");

        GetTaskTypesEndpoint.Map(taskTypes);
        CreateTaskTypeEndpoint.Map(taskTypes);
        DeleteTaskTypeEndpoint.Map(taskTypes);
    }
}