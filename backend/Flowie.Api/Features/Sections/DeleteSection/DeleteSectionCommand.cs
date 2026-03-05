using MediatR;

namespace Flowie.Api.Features.Sections.DeleteSection;

public record DeleteSectionCommand(int SectionId) : IRequest<Unit>;
