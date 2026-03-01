using MediatR;

namespace Flowie.Api.Features.Sections.CreateSection;

public record CreateSectionCommand(
    int ProjectId,
    string Title,
    string? Description) : IRequest<Unit>;
