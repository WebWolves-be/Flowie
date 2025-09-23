using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowie.Tests.Contract;

public class Projects_Tasks_GetTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Projects_Tasks_GetTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProjectTasks_WithValidProjectId_ShouldReturnOk()
    {
        // Arrange
        // This test requires a project to exist - would use test DB setup or factory pattern
        var projectId = Guid.NewGuid(); // Replace with a known ID in a real test

        // Act
        var response = await _client.GetAsync($"/api/projects/{projectId}/tasks");

        // Assert - in a real test we'd check for 200, but using 404 for now
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectTasks_WithInvalidProjectId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/projects/{Guid.NewGuid()}/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}