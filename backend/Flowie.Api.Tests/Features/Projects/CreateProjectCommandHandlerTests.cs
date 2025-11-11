using Flowie.Api.Features.Projects.CreateProject;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.Projects;

public class CreateProjectCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateProject_WhenValidCommandProvided()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldCreateProject_WhenValidCommandProvided));
        var handler = new CreateProjectCommandHandler(context);
        var command = new CreateProjectCommand("Test Project", "Test Description", Company.Immoseed);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var project = await context.Projects.FirstOrDefaultAsync();
        Assert.NotNull(project);
        Assert.Equal("Test Project", project.Title);
        Assert.Equal("Test Description", project.Description);
        Assert.Equal(Company.Immoseed, project.Company);
        Assert.False(project.IsDeleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateProject_WithoutDescription()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldCreateProject_WithoutDescription));
        var handler = new CreateProjectCommandHandler(context);
        var command = new CreateProjectCommand("Test Project", null, Company.Immoseed);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var project = await context.Projects.FirstOrDefaultAsync();
        Assert.NotNull(project);
        Assert.Equal("Test Project", project.Title);
        Assert.Null(project.Description);
        Assert.Equal(Company.Immoseed, project.Company);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateMultipleProjects()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldCreateMultipleProjects));
        var handler = new CreateProjectCommandHandler(context);
        var command1 = new CreateProjectCommand("Project 1", "Description 1", Company.Immoseed);
        var command2 = new CreateProjectCommand("Project 2", "Description 2", Company.NovaraRealEstate);

        // Act
        await handler.Handle(command1, CancellationToken.None);
        await handler.Handle(command2, CancellationToken.None);

        // Assert
        var projects = await context.Projects.ToListAsync();
        Assert.Equal(2, projects.Count);
        Assert.Contains(projects, p => p.Title == "Project 1");
        Assert.Contains(projects, p => p.Title == "Project 2");
    }
}
