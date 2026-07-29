using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Interfaces;
using StackExchange.Redis;

namespace MikroProje.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly RedisOptions _options;
    private readonly IApplicationMetrics? _metrics;
    
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public RedisCacheService(
        IDistributedCache cache,
        ILogger<RedisCacheService> logger,
        IOptions<RedisOptions> options,
        IConnectionMultiplexer? connectionMultiplexer = null,
        IApplicationMetrics? metrics = null)
    {
        _cache = cache;
        _logger = logger;
        _options = options.Value;
        _connectionMultiplexer = connectionMultiplexer;
        _metrics = metrics;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(GetFullKey(key), cancellationToken);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializer.Deserialize<T>(cachedValue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis Cache okuma hatası. Key: {Key}. Sistem veritabanından devam edecek.", key);
        }

        return default;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        // 1. Try get from cache
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue is not null)
        {
            if (key.StartsWith(CacheKeys.DashboardPrefix)) _metrics?.IncrementDashboardCacheHits(1);
            return cachedValue;
        }

        // 2. Lock to prevent cache stampede
        var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            // Re-check after acquiring lock
            cachedValue = await GetAsync<T>(key, cancellationToken);
            if (cachedValue is not null)
            {
                if (key.StartsWith(CacheKeys.DashboardPrefix)) _metrics?.IncrementDashboardCacheHits(1);
                return cachedValue;
            }

            if (key.StartsWith(CacheKeys.DashboardPrefix)) _metrics?.IncrementDashboardCacheMisses(1);

            // 3. Execute factory
            var value = await factory(cancellationToken);

            // 4. Do not cache null or failed results
            if (value is not null && IsSuccessResult(value))
            {
                await SetAsync(key, value, expiration, cancellationToken);
            }

            return value;
        }
        finally
        {
            semaphore.Release();
            // Optional: Remove semaphore to prevent memory leak, but it requires careful concurrent handling.
            // A simple approach is to remove it if there are no waiters.
            if (semaphore.CurrentCount == 1)
            {
                _semaphores.TryRemove(key, out _);
            }
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(GetFullKey(key), serializedValue, cacheOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis Cache yazma hatası. Key: {Key}.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(GetFullKey(key), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis Cache silme hatası. Key: {Key}.", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connectionMultiplexer == null)
            {
                _logger.LogWarning("RemoveByPrefixAsync çağrıldı ancak IConnectionMultiplexer null.");
                return;
            }

            var fullPrefix = $"{_options.InstanceName}{prefix}";
            var endpoints = _connectionMultiplexer.GetEndPoints();
            var db = _connectionMultiplexer.GetDatabase();

            foreach (var endPoint in endpoints)
            {
                var server = _connectionMultiplexer.GetServer(endPoint);
                if (!server.IsConnected) continue;

                // IServer.KeysAsync requires the exact prefix in Redis (including InstanceName)
                var keys = server.KeysAsync(pattern: fullPrefix + "*");
                
                await foreach (var key in keys.WithCancellation(cancellationToken))
                {
                    await db.KeyDeleteAsync(key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis Cache prefix bazlı silme hatası. Prefix: {Prefix}.", prefix);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _cache.GetAsync(GetFullKey(key), cancellationToken);
            return cachedValue != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis Cache Exists kontrol hatası. Key: {Key}.", key);
            return false;
        }
    }

    // IDistributedCache already adds the InstanceName automatically.
    private string GetFullKey(string key) => key;


    private bool IsSuccessResult<T>(T value)
    {
        // Result<T> pattern check - assume it's successful if it doesn't have a Success property that is false
        var successProp = value?.GetType().GetProperty("Success");
        if (successProp != null)
        {
            var successValue = successProp.GetValue(value);
            if (successValue is bool isSuccess)
            {
                return isSuccess;
            }
        }
        return true;
    }
}
