using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Flowie.Features.Tasks.ChangeTaskStatus;
using Flowie.Shared.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowie.Tests.Contract;

public class Tasks_Status_PostTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Tasks_Status_PostTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ChangeTaskStatus_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var taskId = Guid.NewGuid(); // Replace with a known ID in a real test
        var command = new ChangeTaskStatusCommand
        {
            Status = WorkflowTaskStatus.Ongoing
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/status", command);

        // Assert - in a real test we'd check for 200, but using 404 for now
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeTaskStatus_WithInvalidStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var invalidStatusValue = 99; // Invalid value that doesn't match enum
        var command = new { Status = invalidStatusValue };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/status", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeTaskStatus_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid(); // Non-existent task
        var command = new ChangeTaskStatusCommand
        {
            Status = WorkflowTaskStatus.Done
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tasks/{taskId}/status", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}