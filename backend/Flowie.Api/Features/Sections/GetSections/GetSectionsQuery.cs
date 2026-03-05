using MediatR;

namespace Flowie.Api.Features.Sections.GetSections;

internal record GetSectionsQuery(int ProjectId) : IRequest<GetSectionsQueryResult>;
