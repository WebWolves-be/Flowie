using Flowie.Api.Features.Projects.UpdateProject;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;

namespace Flowie.Api.Tests.Features.Projects;

public class UpdateProjectCommandHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly UpdateProjectCommandHandler _sut;

    public UpdateProjectCommandHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _sut = new UpdateProjectCommandHandler(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateProject_WhenProjectExists()
    {
        // Arrange
        var project = new Project
        {
            Title = "Original Title",
            Description = "Original Description",
            Company = Company.Immoseed
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var command = new UpdateProjectCommand(project.Id, "Updated Title", "Updated Description", Company.NovaraRealEstate);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedProject = await _context.Projects.FindAsync(project.Id);
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Title", updatedProject.Title);
        Assert.Equal("Updated Description", updatedProject.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenProjectDoesNotExist()
    {
        // Arrange
        var command = new UpdateProjectCommand(999, "Updated Title", "Updated Description", Company.Immoseed);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await _sut.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateProjectWithNullDescription()
    {
        // Arrange
        var project = new Project
        {
            Title = "Original Title",
            Description = "Original Description",
            Company = Company.NovaraRealEstate
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var command = new UpdateProjectCommand(project.Id, "Updated Title", string.Empty, Company.NovaraRealEstate);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedProject = await _context.Projects.FindAsync(project.Id);
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Title", updatedProject.Title);
        Assert.Equal(string.Empty, updatedProject.Description);
    }
}
