using MediatR;

namespace Flowie.Api.Shared.Infrastructure.Behaviors;

internal static class MediatorBehaviorExtensions
{
    public static void AddMediatorBehaviors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}