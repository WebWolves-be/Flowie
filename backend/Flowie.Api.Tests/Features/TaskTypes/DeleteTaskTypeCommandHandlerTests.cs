using Flowie.Api.Features.TaskTypes.DeleteTaskType;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.TaskTypes;

public class DeleteTaskTypeCommandHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly DeleteTaskTypeCommandHandler _sut;

    public DeleteTaskTypeCommandHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _sut = new DeleteTaskTypeCommandHandler(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldDeleteTaskType_WhenTaskTypeExists()
    {
        // Arrange
        var taskType = new TaskType { Name = "Bug", Active = true };
        _context.TaskTypes.Add(taskType);
        await _context.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var deletedTaskType = await _context.TaskTypes.FindAsync(taskType.Id);
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
        _context.TaskTypes.AddRange(taskType1, taskType2, taskType3);
        await _context.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType2.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var remainingTaskTypes = await _context.TaskTypes.ToListAsync();
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
        _context.TaskTypes.Add(taskType);
        await _context.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var deletedTaskType = await _context.TaskTypes.FindAsync(taskType.Id);
        Assert.Null(deletedTaskType);
    }
}
