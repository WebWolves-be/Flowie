using MediatR;

namespace Flowie.Api.Features.Sections.ReorderSections;

public record ReorderSectionsCommand(List<ReorderSectionItem> Items) : IRequest<Unit>;

public record ReorderSectionItem(int SectionId, int DisplayOrder);
