using MediatR;
using Microsoft.Extensions.Options;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Dashboard.Queries;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly RedisOptions _redisOptions;

    public GetDashboardSummaryQueryHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IOptions<RedisOptions> redisOptions)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _redisOptions = redisOptions.Value;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.DashboardSummary();
        var expiration = TimeSpan.FromMinutes(_redisOptions.DashboardExpirationMinutes);

        var result = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async (ct) =>
            {
                var now = DateTime.UtcNow; // Or project specific standard
                var todayStart = now.Date;
                var tomorrowStart = todayStart.AddDays(1);
                
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var nextMonthStart = monthStart.AddMonths(1);

                var dbResult = await _dashboardRepository.GetSummaryAsync(todayStart, tomorrowStart, monthStart, nextMonthStart, ct);
                return Result<DashboardSummaryDto>.Ok(dbResult);
            },
            expiration,
            cancellationToken);

        return result;
    }
}
