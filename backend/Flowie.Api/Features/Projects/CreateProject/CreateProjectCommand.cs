using Flowie.Api.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Api.Features.Projects.CreateProject;

public record CreateProjectCommand(
    string Title,
    string? Description,
    Company Company) : IRequest<Unit>;