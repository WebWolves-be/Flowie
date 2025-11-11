using Flowie.Api.Features.Projects.GetProjectById;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;

namespace Flowie.Api.Tests.Features.Projects;

public class GetProjectByIdQueryHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnProject_WhenProjectExists()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldReturnProject_WhenProjectExists));
        var project = new Project
        {
            Title = "Test Project",
            Description = "Test Description",
            Company = Company.Immoseed
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var handler = new GetProjectByIdQueryHandler(context);
        var query = new GetProjectByIdQuery(project.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal("Test Project", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(Company.Immoseed, result.Company);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenProjectDoesNotExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldThrowEntityNotFoundException_WhenProjectDoesNotExist));
        var handler = new GetProjectByIdQueryHandler(context);
        var query = new GetProjectByIdQuery(999);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(query, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnProjectWithNullDescription()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldReturnProjectWithNullDescription));
        var project = new Project
        {
            Title = "Test Project",
            Description = null,
            Company = Company.NovaraRealEstate
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var handler = new GetProjectByIdQueryHandler(context);
        var query = new GetProjectByIdQuery(project.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal("Test Project", result.Title);
        Assert.Null(result.Description);
        Assert.Equal(Company.NovaraRealEstate, result.Company);
    }
}
