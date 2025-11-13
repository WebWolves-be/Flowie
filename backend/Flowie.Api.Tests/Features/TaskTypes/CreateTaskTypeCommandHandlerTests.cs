using Flowie.Api.Features.TaskTypes.CreateTaskType;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.TaskTypes;

public class CreateTaskTypeCommandHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly CreateTaskTypeCommandHandler _sut;

    public CreateTaskTypeCommandHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _sut = new CreateTaskTypeCommandHandler(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateTaskType_WhenValidCommandProvided()
    {
        // Arrange
        var command = new CreateTaskTypeCommand("Bug");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var taskType = await _context.TaskTypes.FirstOrDefaultAsync();
        Assert.NotNull(taskType);
        Assert.Equal("Bug", taskType.Name);
        Assert.True(taskType.Active);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateMultipleTaskTypes()
    {
        // Arrange
        var command1 = new CreateTaskTypeCommand("Bug");
        var command2 = new CreateTaskTypeCommand("Feature");
        var command3 = new CreateTaskTypeCommand("Refactor");

        // Act
        await _sut.Handle(command1, CancellationToken.None);
        await _sut.Handle(command2, CancellationToken.None);
        await _sut.Handle(command3, CancellationToken.None);

        // Assert
        var taskTypes = await _context.TaskTypes.ToListAsync();
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
        var command = new CreateTaskTypeCommand("Documentation");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var taskType = await _context.TaskTypes.FirstOrDefaultAsync();
        Assert.NotNull(taskType);
        Assert.True(taskType.Active);
    }
}
