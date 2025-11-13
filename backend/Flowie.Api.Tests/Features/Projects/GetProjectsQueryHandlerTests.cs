using Flowie.Api.Features.Projects.GetProjects;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Tests.Helpers;

namespace Flowie.Api.Tests.Features.Projects;

public class GetProjectsQueryHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly GetProjectsQueryHandler _sut;

    public GetProjectsQueryHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _sut = new GetProjectsQueryHandler(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnAllProjects_WhenProjectsExist()
    {
        // Arrange
        var projects = new[]
        {
            new Project { Title = "Project 1", Description = "Description 1", Company = Company.Immoseed },
            new Project { Title = "Project 2", Description = "Description 2", Company = Company.NovaraRealEstate },
            new Project { Title = "Project 3", Description = null, Company = Company.Immoseed }
        };
        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();

        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Projects.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnEmptyList_WhenNoProjectsExist()
    {
        // Arrange
        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Projects);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldFilterByCompany_WhenCompanySpecified()
    {
        // Arrange
        var projects = new[]
        {
            new Project { Title = "Project 1", Company = Company.Immoseed },
            new Project { Title = "Project 2", Company = Company.NovaraRealEstate },
            new Project { Title = "Project 3", Company = Company.Immoseed }
        };
        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();

        var query = new GetProjectsQuery(Company.Immoseed);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Projects.Count);
        Assert.All(result.Projects, p => Assert.Equal(Company.Immoseed, p.Company));
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnAllProjects_IncludingDeleted()
    {
        // Arrange
        var projects = new[]
        {
            new Project { Title = "Active Project", Company = Company.Immoseed, IsDeleted = false },
            new Project { Title = "Deleted Project", Company = Company.Immoseed, IsDeleted = true }
        };
        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();

        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Projects.Count);
    }
}
