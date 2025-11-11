using Flowie.Api.Features.TaskTypes.DeleteTaskType;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.TaskTypes;

public class DeleteTaskTypeCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldDeleteTaskType_WhenTaskTypeExists()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldDeleteTaskType_WhenTaskTypeExists));
        var taskType = new TaskType { Name = "Bug", Active = true };
        context.TaskTypes.Add(taskType);
        await context.SaveChangesAsync();

        var handler = new DeleteTaskTypeCommandHandler(context);
        var command = new DeleteTaskTypeCommand(taskType.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedTaskType = await context.TaskTypes.FindAsync(taskType.Id);
        Assert.Null(deletedTaskType);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskTypeDoesNotExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldThrowEntityNotFoundException_WhenTaskTypeDoesNotExist));
        var handler = new DeleteTaskTypeCommandHandler(context);
        var command = new DeleteTaskTypeCommand(999);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldOnlyDeleteSpecifiedTaskType()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldOnlyDeleteSpecifiedTaskType));
        var taskType1 = new TaskType { Name = "Bug", Active = true };
        var taskType2 = new TaskType { Name = "Feature", Active = true };
        var taskType3 = new TaskType { Name = "Refactor", Active = true };
        context.TaskTypes.AddRange(taskType1, taskType2, taskType3);
        await context.SaveChangesAsync();

        var handler = new DeleteTaskTypeCommandHandler(context);
        var command = new DeleteTaskTypeCommand(taskType2.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingTaskTypes = await context.TaskTypes.ToListAsync();
        Assert.Equal(2, remainingTaskTypes.Count);
        Assert.Contains(remainingTaskTypes, t => t.Name == "Bug");
        Assert.Contains(remainingTaskTypes, t => t.Name == "Refactor");
        Assert.DoesNotContain(remainingTaskTypes, t => t.Name == "Feature");
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldDeleteInactiveTaskType()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldDeleteInactiveTaskType));
        var taskType = new TaskType { Name = "Deprecated", Active = false };
        context.TaskTypes.Add(taskType);
        await context.SaveChangesAsync();

        var handler = new DeleteTaskTypeCommandHandler(context);
        var command = new DeleteTaskTypeCommand(taskType.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedTaskType = await context.TaskTypes.FindAsync(taskType.Id);
        Assert.Null(deletedTaskType);
    }
}
