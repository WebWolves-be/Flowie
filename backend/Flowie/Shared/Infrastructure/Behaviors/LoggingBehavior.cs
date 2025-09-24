using System.Diagnostics;
using MediatR;

namespace Flowie.Shared.Infrastructure.Behaviors;

internal class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    // Define static LoggerMessage delegates for better performance
    private static readonly Action<ILogger, string, Exception?> _handlingRequest =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(Handle)), "Handling {RequestName}");
    
    private static readonly Action<ILogger, string, long, Exception?> _handledRequest =
        LoggerMessage.Define<string, long>(LogLevel.Information, new EventId(2, nameof(Handle)), 
            "Handled {RequestName} in {ElapsedMilliseconds}ms");

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);
        
        var requestName = typeof(TRequest).Name;
        _handlingRequest(_logger, requestName, null);
        
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
        
        _handledRequest(_logger, requestName, stopwatch.ElapsedMilliseconds, null);
        
        return response;
    }
}
