using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
using MikroProje.API.Options;

namespace MikroProje.API.Extensions;

public static class RateLimitPolicies
{
    public const string Login = "Login";
}

public static class RateLimiterExtensions
{
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitingOptions = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>();
        
        if (rateLimitingOptions == null || rateLimitingOptions.Global.PermitLimit <= 0)
        {
            throw new InvalidOperationException("Rate limiting configuration is missing or invalid.");
        }

        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var rateLimitingOptions = httpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<RateLimitingOptions>>().Value;
                
                var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(ip))
                {
                    ip = httpContext.Connection.RemoteIpAddress?.ToString();
                }
                ip ??= "unknown";
                
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitingOptions.Global.PermitLimit,
                    Window = TimeSpan.FromSeconds(rateLimitingOptions.Global.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitingOptions.Global.QueueLimit
                });
            });

            options.AddPolicy(RateLimitPolicies.Login, httpContext =>
            {
                var rateLimitingOptions = httpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<RateLimitingOptions>>().Value;

                var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(ip))
                {
                    ip = httpContext.Connection.RemoteIpAddress?.ToString();
                }
                ip ??= "unknown";
                
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitingOptions.Login.PermitLimit,
                    Window = TimeSpan.FromSeconds(rateLimitingOptions.Login.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitingOptions.Login.QueueLimit
                });
            });

            options.OnRejected = async (context, token) =>
            {
                var rateLimitingOptions = context.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<RateLimitingOptions>>().Value;
                var metrics = context.HttpContext.RequestServices.GetService<MikroProje.Application.Interfaces.IApplicationMetrics>();
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RateLimiter");

                metrics?.IncrementRateLimitRejections(1);
                logger.LogWarning("Rate limit exceeded for path {Path}", context.HttpContext.Request.Path.Value);

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }
                else
                {
                    context.HttpContext.Response.Headers.RetryAfter = rateLimitingOptions.Global.WindowSeconds.ToString();
                }

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Çok Fazla İstek",
                    Detail = "Sistem limitlerini aştınız. Lütfen daha sonra tekrar deneyiniz.",
                    Instance = context.HttpContext.Request.Path
                };

                var json = System.Text.Json.JsonSerializer.Serialize(problemDetails);
                await context.HttpContext.Response.WriteAsync(json, token);
            };
        });

        return services;
    }
}
