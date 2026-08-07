using MediatR;
using Microsoft.Extensions.Options;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Dashboard.Queries;

public class GetCriticalStockQueryHandler : IRequestHandler<GetCriticalStockQuery, Result<List<DashboardCriticalStockDto>>>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly RedisOptions _redisOptions;

    public GetCriticalStockQueryHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IOptions<RedisOptions> redisOptions)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _redisOptions = redisOptions.Value;
    }

    public async Task<Result<List<DashboardCriticalStockDto>>> Handle(GetCriticalStockQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.DashboardCriticalStock();
        var expiration = TimeSpan.FromMinutes(_redisOptions.DashboardExpirationMinutes);

        var result = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async (ct) =>
            {
                var dbResult = await _dashboardRepository.GetCriticalStockAsync(ct);
                return Result<List<DashboardCriticalStockDto>>.Ok(dbResult);
            },
            expiration,
            cancellationToken);

        return result;
    }
}
