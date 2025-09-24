using Flowie.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Features.Projects.GetProjects;

internal record GetProjectsQuery(Company? Company = null) : IRequest<List<GetProjectsQueryResult>>;