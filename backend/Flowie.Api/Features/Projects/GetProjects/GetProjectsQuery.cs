using Flowie.Api.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Api.Features.Projects.GetProjects;

internal record GetProjectsQuery(Company? Company = null) : IRequest<List<GetProjectsQueryResult>>;