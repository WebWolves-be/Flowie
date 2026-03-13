using MediatR;

namespace Flowie.Api.Features.Projects.DeleteProject;

public record DeleteProjectCommand(int ProjectId) : IRequest<Unit>;
