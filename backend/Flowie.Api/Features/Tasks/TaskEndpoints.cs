using Flowie.Api.Features.Tasks.CreateTask;
using Flowie.Api.Features.Tasks.DeleteTask;
using Flowie.Api.Features.Tasks.GetTaskById;
using Flowie.Api.Features.Tasks.GetTasks;
using Flowie.Api.Features.Tasks.UpdateTask;
using Flowie.Api.Features.Tasks.UpdateTaskStatus;

namespace Flowie.Api.Features.Tasks;

internal static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var tasks = app
            .MapGroup("/api/tasks")
            //.RequireAuthorization()
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