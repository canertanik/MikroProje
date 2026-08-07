using MediatR;
using Microsoft.Extensions.Options;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Dashboard.Queries;

public class GetRecentActivitiesQueryHandler : IRequestHandler<GetRecentActivitiesQuery, Result<List<DashboardRecentActivityDto>>>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly RedisOptions _redisOptions;

    public GetRecentActivitiesQueryHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IOptions<RedisOptions> redisOptions)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _redisOptions = redisOptions.Value;
    }

    public async Task<Result<List<DashboardRecentActivityDto>>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.DashboardActivities();
        var expiration = TimeSpan.FromMinutes(_redisOptions.DashboardExpirationMinutes);

        var result = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async (ct) =>
            {
                var dbResult = await _dashboardRepository.GetRecentActivitiesAsync(ct);
                return Result<List<DashboardRecentActivityDto>>.Ok(dbResult);
            },
            expiration,
            cancellationToken);

        return result;
    }
}
