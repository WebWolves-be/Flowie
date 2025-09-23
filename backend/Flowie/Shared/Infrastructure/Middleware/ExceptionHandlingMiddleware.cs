using FluentValidation;
using Flowie.Shared.Domain.Exceptions;

namespace Flowie.Infrastructure.Middleware;

internal class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    // Define static LoggerMessage delegate for better performance
    private static readonly Action<ILogger, Exception> _logError =
        LoggerMessage.Define(LogLevel.Error, new EventId(1, nameof(InvokeAsync)), 
            "An unhandled exception occurred");

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        
        try
        {
            await _next(httpContext);
        }
        catch (ValidationException ex)
        {
            // Log and handle specific known exception
            _logError(_logger, ex);
            await HandleExceptionAsync(httpContext, ex);
        }
        catch (KeyNotFoundException ex)
        {
            // Log and handle specific known exception
            _logError(_logger, ex);
            await HandleExceptionAsync(httpContext, ex);
        }
        catch (DomainException ex)
        {
            // Log and handle domain-specific exception
            _logError(_logger, ex);
            await HandleExceptionAsync(httpContext, ex);
        }
        catch (Exception ex) when (LogAndHandleException(ex))
        {
            // This code is never reached because LogAndHandleException always returns false
            // This pattern allows us to log and swallow exceptions in one go
            await HandleExceptionAsync(httpContext, ex);
        }
    }
    
    private bool LogAndHandleException(Exception ex)
    {
        // Log the exception
        _logError(_logger, ex);
        // Return false to continue with the exception handling
        return false;
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = exception switch
        {
            ValidationException _ => StatusCodes.Status400BadRequest,
            KeyNotFoundException _ => StatusCodes.Status404NotFound,
            TaskNotFoundException _ => StatusCodes.Status404NotFound,
            ProjectNotFoundException _ => StatusCodes.Status404NotFound,
            TaskTypeNotFoundException _ => StatusCodes.Status404NotFound,
            EmployeeNotFoundException _ => StatusCodes.Status404NotFound,
            ParentTaskNotFoundException _ => StatusCodes.Status404NotFound,
            TaskWithSubtasksException _ => StatusCodes.Status400BadRequest,
            TaskTypeInUseException _ => StatusCodes.Status400BadRequest,
            TaskTypeAlreadyExistsException _ => StatusCodes.Status400BadRequest,
            ParentTaskProjectMismatchException _ => StatusCodes.Status400BadRequest,
            ConfigurationException _ => StatusCodes.Status500InternalServerError,
            DomainException _ => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        
        context.Response.StatusCode = statusCode;
        
        var response = new
        {
            Status = statusCode,
            Message = exception.Message,
            Errors = exception is ValidationException validationException
                ? validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                : null
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
}
