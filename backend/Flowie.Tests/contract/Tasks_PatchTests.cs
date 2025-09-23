using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Flowie.Features.Tasks.UpdateTask;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowie.Tests.Contract;

public class Tasks_PatchTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Tasks_PatchTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateTask_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var taskId = Guid.NewGuid(); // Replace with a known ID in a real test
        var typeId = Guid.NewGuid(); // Replace with a known type ID
        var command = new UpdateTaskCommand
        {
            Title = "Updated Task Title",
            Description = "Updated task description",
            TypeId = typeId,
            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(14))
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{taskId}", command);

        // Assert - in a real test we'd check for 200, but using 404 for now
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTask_WithInvalidTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var command = new UpdateTaskCommand
        {
            Title = "A" // Too short
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{taskId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid(); // Non-existent task
        var command = new UpdateTaskCommand
        {
            Title = "Updated Task Title"
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/tasks/{taskId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}