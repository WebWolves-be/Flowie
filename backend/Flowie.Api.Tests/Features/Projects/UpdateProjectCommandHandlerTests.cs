using Flowie.Api.Features.Projects.UpdateProject;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;

namespace Flowie.Api.Tests.Features.Projects;

public class UpdateProjectCommandHandlerTests : BaseTestClass
{
    private readonly UpdateProjectCommandHandler _sut;

    public UpdateProjectCommandHandlerTests()
    {
        _sut = new UpdateProjectCommandHandler(DatabaseContext);
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
        DatabaseContext.Projects.Add(project);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(project.Id, "Updated Title", "Updated Description", Company.NovaraRealEstate);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedProject = await DatabaseContext.Projects.FindAsync(project.Id);
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
        DatabaseContext.Projects.Add(project);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(project.Id, "Updated Title", string.Empty, Company.NovaraRealEstate);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedProject = await DatabaseContext.Projects.FindAsync(project.Id);
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Title", updatedProject.Title);
        Assert.Equal(string.Empty, updatedProject.Description);
    }
}
