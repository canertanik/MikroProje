using System.Net;
using System.Text.Json;
using MikroProje.Application.Common.Exceptions;

namespace MikroProje.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogError(ex, "Concurrency conflict occurred.");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";
            
            var result = JsonSerializer.Serialize(new 
            { 
                statusCode = StatusCodes.Status409Conflict,
                message = ex.Message,
                error = "ConcurrencyConflict"
            });
            
            await context.Response.WriteAsync(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var result = JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "One or more validation errors occurred.",
                status = StatusCodes.Status400BadRequest,
                errors = errors
            });

            await context.Response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            // TraceId is already added by OpenTelemetry and Serilog (as trace_id). 
            // We can fetch Activity.Current?.Id as TraceId
            var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;

            var result = JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                title = "An unexpected error occurred.",
                status = StatusCodes.Status500InternalServerError,
                detail = "İşleminiz sırasında beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.",
                traceId = traceId
            });

            await context.Response.WriteAsync(result);
        }
    }
}
