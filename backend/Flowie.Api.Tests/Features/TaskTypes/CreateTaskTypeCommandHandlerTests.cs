using Flowie.Api.Features.TaskTypes.CreateTaskType;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.TaskTypes;

public class CreateTaskTypeCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateTaskType_WhenValidCommandProvided()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldCreateTaskType_WhenValidCommandProvided));
        var handler = new CreateTaskTypeCommandHandler(context);
        var command = new CreateTaskTypeCommand("Bug");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var taskType = await context.TaskTypes.FirstOrDefaultAsync();
        Assert.NotNull(taskType);
        Assert.Equal("Bug", taskType.Name);
        Assert.True(taskType.Active);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateMultipleTaskTypes()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldCreateMultipleTaskTypes));
        var handler = new CreateTaskTypeCommandHandler(context);
        var command1 = new CreateTaskTypeCommand("Bug");
        var command2 = new CreateTaskTypeCommand("Feature");
        var command3 = new CreateTaskTypeCommand("Refactor");

        // Act
        await handler.Handle(command1, CancellationToken.None);
        await handler.Handle(command2, CancellationToken.None);
        await handler.Handle(command3, CancellationToken.None);

        // Assert
        var taskTypes = await context.TaskTypes.ToListAsync();
        Assert.Equal(3, taskTypes.Count);
        Assert.Contains(taskTypes, t => t.Name == "Bug");
        Assert.Contains(taskTypes, t => t.Name == "Feature");
        Assert.Contains(taskTypes, t => t.Name == "Refactor");
        Assert.All(taskTypes, t => Assert.True(t.Active));
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldSetActiveToTrue_ByDefault()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldSetActiveToTrue_ByDefault));
        var handler = new CreateTaskTypeCommandHandler(context);
        var command = new CreateTaskTypeCommand("Documentation");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var taskType = await context.TaskTypes.FirstOrDefaultAsync();
        Assert.NotNull(taskType);
        Assert.True(taskType.Active);
    }
}
