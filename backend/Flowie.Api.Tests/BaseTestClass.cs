using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Tests.Helpers;

namespace Flowie.Api.Tests;

public abstract class BaseTestClass : IDisposable
{
    protected readonly DatabaseContext DatabaseContext;

    protected BaseTestClass()
    {
        DatabaseContext = Helpers.DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
    }

    public void Dispose()
    {
        DatabaseContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
