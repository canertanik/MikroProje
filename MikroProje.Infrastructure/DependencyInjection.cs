using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Interfaces;
using MikroProje.Infrastructure.Caching;
using MikroProje.Infrastructure.Services;
using StackExchange.Redis;

namespace MikroProje.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        var redisOptions = new RedisOptions();
        configuration.GetSection(RedisOptions.SectionName).Bind(redisOptions);
        
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

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

        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddSingleton<IApplicationMetrics, MikroProje.Infrastructure.Observability.MikroProjeMetrics>();

        return services;
    }
}
