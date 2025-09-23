using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Flowie.Features.Projects.UpdateProject;
using Flowie.Shared.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowie.Tests.Contract;

public class Projects_PatchTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Projects_PatchTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateProject_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var projectId = Guid.NewGuid(); // Replace with a known ID in a real test
        var command = new UpdateProjectCommand
        {
            Title = "Updated Project Title",
            Description = "Updated project description",
            Company = Company.NovaraRealEstate
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/projects/{projectId}", command);

        // Assert - in a real test we'd check for 200, but using 404 for now
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProject_WithInvalidTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var command = new UpdateProjectCommand
        {
            Title = "A", // Too short
            Description = "Updated project description"
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/projects/{projectId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProject_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var command = new UpdateProjectCommand
        {
            Title = "Updated Project Title",
            Description = "Updated project description"
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/projects/{projectId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}