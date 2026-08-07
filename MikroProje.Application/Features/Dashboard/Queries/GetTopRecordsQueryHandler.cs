using MediatR;
using Microsoft.Extensions.Options;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Dashboard.Queries;

public class GetTopRecordsQueryHandler : IRequestHandler<GetTopRecordsQuery, Result<DashboardTopRecordsDto>>
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly RedisOptions _redisOptions;

    public GetTopRecordsQueryHandler(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IOptions<RedisOptions> redisOptions)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _redisOptions = redisOptions.Value;
    }

    public async Task<Result<DashboardTopRecordsDto>> Handle(GetTopRecordsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.DashboardTopRecords(request.StartDate, request.EndDate);
        var expiration = TimeSpan.FromMinutes(_redisOptions.DashboardExpirationMinutes);

        var result = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async (ct) =>
            {
                var dbResult = await _dashboardRepository.GetTopRecordsAsync(request.StartDate, request.EndDate, ct);
                return Result<DashboardTopRecordsDto>.Ok(dbResult);
            },
            expiration,
            cancellationToken);

        return result;
    }
}
