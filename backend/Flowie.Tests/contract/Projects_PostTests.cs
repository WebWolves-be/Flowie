using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Flowie.Features.Projects.CreateProject;
using Flowie.Shared.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowie.Tests.Contract;

public class Projects_PostTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Projects_PostTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProject_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Title = "New Test Project",
            Description = "Project for testing",
            Company = Company.Immoseed
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProject_WithInvalidTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Title = "AB", // Too short - minimum 3 characters
            Description = "Project for testing",
            Company = Company.Immoseed
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProject_WithMissingCompany_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Title = "New Test Project",
            Description = "Project for testing"
            // Missing Company field
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}