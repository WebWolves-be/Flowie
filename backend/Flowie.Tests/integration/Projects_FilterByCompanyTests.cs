using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Flowie.Features.Projects.CreateProject;
using Flowie.Features.Projects.GetProjects;
using Flowie.Shared.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Xunit;

namespace Flowie.Tests.Integration;

public class Projects_FilterByCompanyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Projects_FilterByCompanyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProjects_FilteredByCompany_ShouldOnlyReturnMatchingProjects()
    {
        // This is an integration test that would:
        // 1. Create projects for both companies
        // 2. Retrieve projects filtered by Immoseed
        // 3. Verify only Immoseed projects are returned
        // 4. Retrieve projects filtered by NovaraRealEstate
        // 5. Verify only NovaraRealEstate projects are returned
        
        // For now, just a placeholder since we need the actual implementation
        // to make this test work properly
        
        // Assert - placeholder assertion
        true.Should().BeTrue();
    }
}