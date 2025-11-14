using Flowie.Api.Features.TaskTypes.DeleteTaskType;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.TaskTypes;

public class DeleteTaskTypeCommandHandlerTests : BaseTestClass
{
    private readonly DeleteTaskTypeCommandHandler _sut;

    public DeleteTaskTypeCommandHandlerTests()
    {
        _sut = new DeleteTaskTypeCommandHandler(DatabaseContext);
    }


    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldDeleteTaskType_WhenTaskTypeExists()
    {
        // Arrange
        var taskType = new TaskType { Name = "Bug", Active = true };
        DatabaseContext.TaskTypes.Add(taskType);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var deletedTaskType = await DatabaseContext.TaskTypes.FindAsync(taskType.Id);
        Assert.Null(deletedTaskType);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskTypeDoesNotExist()
    {
        // Arrange
        var command = new DeleteTaskTypeCommand(999);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await _sut.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldOnlyDeleteSpecifiedTaskType()
    {
        // Arrange
        var taskType1 = new TaskType { Name = "Bug", Active = true };
        var taskType2 = new TaskType { Name = "Feature", Active = true };
        var taskType3 = new TaskType { Name = "Refactor", Active = true };
        DatabaseContext.TaskTypes.AddRange(taskType1, taskType2, taskType3);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType2.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var remainingTaskTypes = await DatabaseContext.TaskTypes.ToListAsync();
        Assert.Equal(2, remainingTaskTypes.Count);
        Assert.Contains(remainingTaskTypes, t => t.Name == "Bug");
        Assert.Contains(remainingTaskTypes, t => t.Name == "Refactor");
        Assert.DoesNotContain(remainingTaskTypes, t => t.Name == "Feature");
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldDeleteInactiveTaskType()
    {
        // Arrange
        var taskType = new TaskType { Name = "Deprecated", Active = false };
        DatabaseContext.TaskTypes.Add(taskType);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var deletedTaskType = await DatabaseContext.TaskTypes.FindAsync(taskType.Id);
        Assert.Null(deletedTaskType);
    }
}
