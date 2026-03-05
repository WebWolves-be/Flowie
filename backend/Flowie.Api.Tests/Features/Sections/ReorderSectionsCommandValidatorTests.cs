using Flowie.Api.Features.Sections.ReorderSections;
using FluentValidation.TestHelper;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Sections;

public class ReorderSectionsCommandValidatorTests
{
    private readonly ReorderSectionsCommandValidator _validator;

    public ReorderSectionsCommandValidatorTests()
    {
        _validator = new ReorderSectionsCommandValidator();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ValidCommand_PassesValidation()
    {
        var items = new List<ReorderSectionItem>
        {
            new(1, 0),
            new(2, 1)
        };
        var command = new ReorderSectionsCommand(items);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_EmptyItems_FailsValidation()
    {
        var command = new ReorderSectionsCommand(new List<ReorderSectionItem>());

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Items zijn verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_NegativeDisplayOrder_FailsValidation()
    {
        var items = new List<ReorderSectionItem>
        {
            new(1, -1)
        };
        var command = new ReorderSectionsCommand(items);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor("Items[0]")
            .WithErrorMessage("DisplayOrder moet groter of gelijk aan 0 zijn.");
    }
}
