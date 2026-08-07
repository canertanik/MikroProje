using MediatR;
using Microsoft.Extensions.Options;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Dashboard.Queries;

public class GetDashboardTrendsQueryHandler : IRequestHandler<GetDashboardTrendsQuery, Result<List<DashboardTrendsDto>>>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly RedisOptions _redisOptions;

    public GetDashboardTrendsQueryHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IOptions<RedisOptions> redisOptions)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _redisOptions = redisOptions.Value;
    }

    public async Task<Result<List<DashboardTrendsDto>>> Handle(GetDashboardTrendsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.DashboardTrends(request.StartDate, request.EndDate);
        var expiration = TimeSpan.FromMinutes(_redisOptions.DashboardExpirationMinutes);

        var result = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async (ct) =>
            {
                var dbResult = await _dashboardRepository.GetTrendsAsync(request.StartDate, request.EndDate, ct);
                return Result<List<DashboardTrendsDto>>.Ok(dbResult);
            },
            expiration,
            cancellationToken);

        return result;
    }
}
