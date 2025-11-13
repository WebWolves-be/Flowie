using Flowie.Api.Features.Projects.GetProjectById;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;

namespace Flowie.Api.Tests.Features.Projects;

public class GetProjectByIdQueryHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly GetProjectByIdQueryHandler _sut;

    public GetProjectByIdQueryHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _sut = new GetProjectByIdQueryHandler(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnProject_WhenProjectExists()
    {
        // Arrange
        var project = new Project
        {
            Title = "Test Project",
            Description = "Test Description",
            Company = Company.Immoseed
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var query = new GetProjectByIdQuery(project.Id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

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
        var query = new GetProjectByIdQuery(999);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await _sut.Handle(query, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnProjectWithNullDescription()
    {
        // Arrange
        var project = new Project
        {
            Title = "Test Project",
            Description = null,
            Company = Company.NovaraRealEstate
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var query = new GetProjectByIdQuery(project.Id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal("Test Project", result.Title);
        Assert.Null(result.Description);
        Assert.Equal(Company.NovaraRealEstate, result.Company);
    }
}
