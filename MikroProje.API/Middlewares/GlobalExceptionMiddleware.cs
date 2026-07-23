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
        // Diğer hatalar uygulamanın mevcut exception handler'ına (DeveloperExceptionPage) gitmeye devam eder.
    }
}
