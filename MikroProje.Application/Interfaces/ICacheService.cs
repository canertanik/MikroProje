namespace MikroProje.Application.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task RemoveByPrefixAsync(
        string prefix,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}
