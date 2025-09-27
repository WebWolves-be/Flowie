using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;

namespace Flowie.Api.Features.TaskTypes.CreateTaskType;

internal class CreateTaskTypeCommandHandler(DatabaseContext dbContext)
    : IRequestHandler<CreateTaskTypeCommand, Unit>
{
    public async Task<Unit> Handle(CreateTaskTypeCommand request, CancellationToken cancellationToken)
    {
        dbContext.TaskTypes.Add(
            new TaskType
            {
                Name = request.Name,
                Active = true
            }
        );

        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}