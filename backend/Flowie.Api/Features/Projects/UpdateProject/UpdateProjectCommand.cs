using Flowie.Api.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Api.Features.Projects.UpdateProject;

public record UpdateProjectCommand(
    int ProjectId, 
    string Title, 
    string Description, 
    Company Company) : IRequest<Unit>;