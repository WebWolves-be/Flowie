using MediatR;

namespace Flowie.Api.Features.Sections.UpdateSection;

public record UpdateSectionCommand(
    int SectionId,
    string Title,
    string? Description) : IRequest<Unit>;
