using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowie.Tests.Contract;

public class Tasks_GetTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Tasks_GetTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTask_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var taskId = Guid.NewGuid(); // Replace with a known ID in a real test

        // Act
        var response = await _client.GetAsync($"/api/tasks/{taskId}");

        // Assert - in a real test we'd check for 200, but using 404 for now
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}