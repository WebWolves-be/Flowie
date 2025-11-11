using Flowie.Api.Features.TaskTypes.GetTaskTypes;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Tests.Helpers;

namespace Flowie.Api.Tests.Features.TaskTypes;

public class GetTaskTypesQueryHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnAllTaskTypes_WhenTaskTypesExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldReturnAllTaskTypes_WhenTaskTypesExist));
        var taskTypes = new[]
        {
            new TaskType { Name = "Bug", Active = true },
            new TaskType { Name = "Feature", Active = true },
            new TaskType { Name = "Refactor", Active = false }
        };
        context.TaskTypes.AddRange(taskTypes);
        await context.SaveChangesAsync();

        var handler = new GetTaskTypesQueryHandler(context);
        var query = new GetTaskTypesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TaskTypes.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnEmptyList_WhenNoTaskTypesExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldReturnEmptyList_WhenNoTaskTypesExist));
        var handler = new GetTaskTypesQueryHandler(context);
        var query = new GetTaskTypesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.TaskTypes);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnBothActiveAndInactiveTaskTypes()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldReturnBothActiveAndInactiveTaskTypes));
        var taskTypes = new[]
        {
            new TaskType { Name = "Active Type", Active = true },
            new TaskType { Name = "Inactive Type", Active = false }
        };
        context.TaskTypes.AddRange(taskTypes);
        await context.SaveChangesAsync();

        var handler = new GetTaskTypesQueryHandler(context);
        var query = new GetTaskTypesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TaskTypes.Count);
        Assert.Contains(result.TaskTypes, t => t.Name == "Active Type");
        Assert.Contains(result.TaskTypes, t => t.Name == "Inactive Type");
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldOrderTaskTypesByName()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldOrderTaskTypesByName));
        var taskTypes = new[]
        {
            new TaskType { Name = "Zebra", Active = true },
            new TaskType { Name = "Alpha", Active = true },
            new TaskType { Name = "Middle", Active = true }
        };
        context.TaskTypes.AddRange(taskTypes);
        await context.SaveChangesAsync();

        var handler = new GetTaskTypesQueryHandler(context);
        var query = new GetTaskTypesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TaskTypes.Count);
        var taskTypesList = result.TaskTypes.ToList();
        Assert.Equal("Alpha", taskTypesList[0].Name);
        Assert.Equal("Middle", taskTypesList[1].Name);
        Assert.Equal("Zebra", taskTypesList[2].Name);
    }
}
