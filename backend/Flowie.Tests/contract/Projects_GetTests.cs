using System.Net;
using FluentAssertions;
using Flowie.Features.Projects.GetProjects;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Xunit;

// Suppress warnings about JSON serialization in test methods
#pragma warning disable IL2026, IL3050

namespace Flowie.Tests.Contract;

public class Projects_GetTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Projects_GetTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProjects_WithoutFilters_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var projects = JsonSerializer.Deserialize<List<ProjectResponse>>(content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        projects.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProjects_WithCompanyFilter_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/projects?company=Immoseed");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var projects = JsonSerializer.Deserialize<List<ProjectResponse>>(content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        projects.Should().NotBeNull();
        projects!.All(p => p.Company == "Immoseed").Should().BeTrue();
    }

    [Fact]
    public async Task GetProjects_WithInvalidCompanyFilter_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/projects?company=InvalidCompany");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

#pragma warning restore IL2026, IL3050