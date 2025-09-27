using Flowie.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Features.Projects.UpdateProject;

internal record UpdateProjectCommand(
    int ProjectId, 
    string Title, 
    string Description, 
    Company Company) : IRequest<Unit>;