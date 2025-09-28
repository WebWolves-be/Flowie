using Flowie.Api.Shared.Infrastructure.Exceptions;
using FluentValidation;

namespace Flowie.Api.Shared.Infrastructure.Middleware;

internal class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
        private static readonly Action<ILogger, Exception> _logError =
        LoggerMessage.Define(LogLevel.Error, new EventId(1, nameof(InvokeAsync)), 
            "An unhandled exception occurred");

    private static readonly Action<ILogger, string, object, Exception?> _logEntityNotFound =
        LoggerMessage.Define<string, object>(LogLevel.Warning, new EventId(2, nameof(InvokeAsync)), 
            "Entity {EntityName} with ID {EntityId} not found");

    public async Task InvokeAsync(HttpContext httpContext)
    {        
        try
        {
            await next(httpContext);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
        catch (EntityNotFoundException ex)
        {
            _logEntityNotFound(logger, ex.EntityName, ex.EntityId, ex);

            await HandleExceptionAsync(httpContext, ex);
        }
        catch (Exception ex)
        {
            _logError(logger, ex);

            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = exception switch
        {
            ValidationException _ => StatusCodes.Status400BadRequest,
            EntityNotFoundException _ => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
        
        context.Response.StatusCode = statusCode;
        
        object response;
        
        if (exception is ValidationException validationException)
        {
            response = new
            {
                Status = statusCode,
                Errors = validationException.Errors.Select(e => new { e.ErrorMessage })
            };
        }
        else if (exception is EntityNotFoundException)
        {
            response = new
            {
                Status = statusCode
            };
        }
        else
        {
            response = new
            {
                Status = statusCode,
            };
        }
        
        await context.Response.WriteAsJsonAsync(response);
    }
}
