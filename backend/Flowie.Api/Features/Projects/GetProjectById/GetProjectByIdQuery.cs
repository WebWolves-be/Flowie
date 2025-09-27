using MediatR;

namespace Flowie.Api.Features.Projects.GetProjectById;

internal record GetProjectByIdQuery(int ProjectId) : IRequest<GetProjectByIdQueryResult>;