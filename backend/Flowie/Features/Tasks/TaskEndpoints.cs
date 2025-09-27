using Flowie.Features.Tasks.CreateTask;
using Flowie.Features.Tasks.DeleteTask;
using Flowie.Features.Tasks.GetTaskById;
using Flowie.Features.Tasks.GetTasks;
using Flowie.Features.Tasks.UpdateTask;
using Flowie.Features.Tasks.UpdateTaskStatus;

namespace Flowie.Features.Tasks;

internal static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var tasks = app
            .MapGroup("/api/projects/{projectId:int}/tasks")
            .WithOpenApi()
            .WithTags("Tasks");

        GetTasksEndpoint.Map(tasks);
        GetTaskByIdEndpoint.Map(tasks);
        CreateTaskEndpoint.Map(tasks);
        UpdateTaskEndpoint.Map(tasks);
        UpdateTaskStatusEndpoint.Map(tasks);
        DeleteTaskEndpoint.Map(tasks);
    }
}