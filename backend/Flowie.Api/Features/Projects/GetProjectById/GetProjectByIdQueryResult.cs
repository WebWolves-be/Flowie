using Flowie.Api.Shared.Domain.Enums;

namespace Flowie.Api.Features.Projects.GetProjectById;

internal record GetProjectByIdQueryResult(
    int ProjectId,
    string Title, 
    string? Description,
    Company Company);