using Flowie.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Features.Projects.UpdateProject;

internal record UpdateProjectCommand(
    int Id, 
    string? Title = null, 
    string? Description = null, 
    Company? Company = null) : IRequest<Unit>;