using FluentValidation;

// Suppress quality rule warnings
#pragma warning disable CA1515, CA1716

namespace Flowie.Features.Projects.GetProjects;

internal class GetProjectsQueryValidator : AbstractValidator<GetProjectsQuery>
{
    public GetProjectsQueryValidator()
    {
        RuleFor(x => x.Company)
            .IsInEnum()
            .When(x => x.Company.HasValue)
            .WithMessage("Company must be Immoseed or NovaraRealEstate");
    }
}