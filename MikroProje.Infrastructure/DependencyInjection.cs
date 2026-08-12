using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Interfaces;
using MikroProje.Infrastructure.Caching;
using MikroProje.Infrastructure.Services;
using StackExchange.Redis;
using Polly;
using Polly.Extensions.Http;

namespace MikroProje.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Register Python ML Service HTTP Client
        var mlBaseUrl = configuration.GetValue<string>("PythonMLService:BaseUrl") ?? "http://localhost:8000";
        services.AddHttpClient<IDemandForecastService, DemandForecastService>(client =>
        {
            client.BaseAddress = new Uri(mlBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy());

        var redisOptions = new RedisOptions();
        configuration.GetSection(RedisOptions.SectionName).Bind(redisOptions);
        
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        if (redisOptions.Enabled)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = redisOptions.InstanceName;
            });

            // Add ConnectionMultiplexer for advanced Redis operations (e.g. RemoveByPrefix)
            try
            {
                var multiplexer = ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
                services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            }
            catch (Exception ex)
            {
                // If Redis is not available at startup, we log it and continue without multiplexer.
                // IDistributedCache will handle its own connection errors.
                Console.WriteLine($"[Redis WARNING] Could not connect to Redis at startup: {ex.Message}");
            }
        }
        else
        {
            // Use In-Memory Cache as a fallback if Redis is disabled
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddSingleton<IApplicationMetrics, MikroProje.Infrastructure.Observability.MikroProjeMetrics>();

        // OpenAI Service Registration
        services.Configure<MikroProje.Infrastructure.Services.OpenAi.OpenAiOptions>(
            configuration.GetSection(MikroProje.Infrastructure.Services.OpenAi.OpenAiOptions.SectionName));

        var openAiOptions = new MikroProje.Infrastructure.Services.OpenAi.OpenAiOptions();
        configuration.GetSection(MikroProje.Infrastructure.Services.OpenAi.OpenAiOptions.SectionName).Bind(openAiOptions);

        if (openAiOptions.Enabled)
        {
            services.AddHttpClient<IOpenAiService, MikroProje.Infrastructure.Services.OpenAi.OpenAiService>(client =>
            {
                client.BaseAddress = new Uri(openAiOptions.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(openAiOptions.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(GetOpenAiRetryPolicy());

            // Register all tool handlers
            services.AddScoped<IErpToolHandler, MikroProje.Infrastructure.Services.OpenAi.Tools.GetDashboardSummaryTool>();
            services.AddScoped<IErpToolHandler, MikroProje.Infrastructure.Services.OpenAi.Tools.GetCriticalStockTool>();
            services.AddScoped<IErpToolHandler, MikroProje.Infrastructure.Services.OpenAi.Tools.GetProductForecastTool>();
            services.AddScoped<IErpToolHandler, MikroProje.Infrastructure.Services.OpenAi.Tools.GetTopSellingProductsTool>();
            services.AddScoped<IErpToolHandler, MikroProje.Infrastructure.Services.OpenAi.Tools.GetRecentActivitiesTool>();
            services.AddScoped<IErpToolHandler, MikroProje.Infrastructure.Services.OpenAi.Tools.GetProductDetailsTool>();
            
            services.AddScoped<MikroProje.Infrastructure.Services.OpenAi.ErpToolRegistry>();
        }
        else
        {
            // OpenAI disabled — register a no-op fallback
            services.AddScoped<IOpenAiService, MikroProje.Infrastructure.Services.OpenAi.DisabledOpenAiService>();
        }

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // 5xx or 408
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetOpenAiRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (retryAttempt, response, context) =>
                {
                    // Respect Retry-After header from OpenAI
                    if (response?.Result?.Headers.RetryAfter?.Delta is TimeSpan delta)
                        return delta;
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                },
                onRetryAsync: (_, _, _, _) => Task.CompletedTask);
    }
}
