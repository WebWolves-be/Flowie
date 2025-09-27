using System.Diagnostics;
using MediatR;

namespace Flowie.Shared.Infrastructure.Behaviors;

internal class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private static readonly Action<ILogger, string, Exception?> _handlingRequest =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(Handle)), "Handling {RequestName}");

    private static readonly Action<ILogger, string, long, Exception?> _handledRequest =
        LoggerMessage.Define<string, long>(LogLevel.Information, new EventId(2, nameof(Handle)),
            "Handled {RequestName} in {ElapsedMilliseconds}ms");

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _handlingRequest(logger, requestName, null);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();

        stopwatch.Stop();

        _handledRequest(logger, requestName, stopwatch.ElapsedMilliseconds, null);

        return response;
    }
}