using Flowie.Api.Features.Projects.UpdateProject;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;

namespace Flowie.Api.Tests.Features.Projects;

public class UpdateProjectCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateProject_WhenProjectExists()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldUpdateProject_WhenProjectExists));
        var project = new Project
        {
            Title = "Original Title",
            Description = "Original Description",
            Company = Company.Immoseed
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var handler = new UpdateProjectCommandHandler(context);
        var command = new UpdateProjectCommand(project.Id, "Updated Title", "Updated Description", Company.NovaraRealEstate);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedProject = await context.Projects.FindAsync(project.Id);
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Title", updatedProject.Title);
        Assert.Equal("Updated Description", updatedProject.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenProjectDoesNotExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldThrowEntityNotFoundException_WhenProjectDoesNotExist));
        var handler = new UpdateProjectCommandHandler(context);
        var command = new UpdateProjectCommand(999, "Updated Title", "Updated Description", Company.Immoseed);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateProjectWithNullDescription()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldUpdateProjectWithNullDescription));
        var project = new Project
        {
            Title = "Original Title",
            Description = "Original Description",
            Company = Company.NovaraRealEstate
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var handler = new UpdateProjectCommandHandler(context);
        var command = new UpdateProjectCommand(project.Id, "Updated Title", string.Empty, Company.NovaraRealEstate);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedProject = await context.Projects.FindAsync(project.Id);
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Title", updatedProject.Title);
        Assert.Equal(string.Empty, updatedProject.Description);
    }
}
