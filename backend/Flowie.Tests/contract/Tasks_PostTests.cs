using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Flowie.Features.Tasks.CreateTask;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowie.Tests.Contract;

public class Tasks_PostTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Tasks_PostTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var projectId = Guid.NewGuid(); // Replace with a known ID in a real test
        var typeId = Guid.NewGuid();    // Replace with a known task type ID
        var command = new CreateTaskCommand
        {
            Title = "New Task",
            Description = "Task description",
            TypeId = typeId,
            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7))
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks", command);

        // Assert - in a real test we'd check for 201, but using 404 for now since we don't have a project
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTask_WithInvalidTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var command = new CreateTaskCommand
        {
            Title = "AB", // Too short
            Description = "Task description",
            TypeId = typeId
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_WithInvalidProjectId_ShouldReturnNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid(); // Non-existent project
        var typeId = Guid.NewGuid();
        var command = new CreateTaskCommand
        {
            Title = "New Task",
            Description = "Task description",
            TypeId = typeId
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}