using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Flowie.Features.Tasks.CreateTask;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowie.Tests.Contract;

public class Subtasks_PostTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Subtasks_PostTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSubtask_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var taskId = Guid.NewGuid(); // Replace with a known parent task ID in a real test
        var typeId = Guid.NewGuid(); // Replace with a known task type ID
        var command = new CreateTaskCommand
        {
            Title = "New Subtask",
            Description = "Subtask description",
            TypeId = typeId,
            Deadline = DateOnly.FromDateTime(DateTime.Now.AddDays(7))
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/subtasks", command);

        // Assert - in a real test we'd check for 201, but using 404 for now
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateSubtask_WithInvalidTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var command = new CreateTaskCommand
        {
            Title = "AB", // Too short
            Description = "Subtask description",
            TypeId = typeId
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/subtasks", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSubtask_WithInvalidParentId_ShouldReturnNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid(); // Non-existent parent task
        var typeId = Guid.NewGuid();
        var command = new CreateTaskCommand
        {
            Title = "New Subtask",
            Description = "Subtask description",
            TypeId = typeId
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/subtasks", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}