using Flowie.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Features.Projects.CreateProject;

internal record CreateProjectCommand(
    string Title,
    string? Description,
    Company Company) : IRequest<Unit>;