using MediatR;

namespace Flowie.Features.Projects.GetProjectById;

internal record GetProjectByIdQuery(int ProjectId) : IRequest<GetProjectByIdQueryResult>;