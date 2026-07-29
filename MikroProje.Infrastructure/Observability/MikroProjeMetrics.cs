using System.Diagnostics.Metrics;
using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Observability;

public class MikroProjeMetrics : IApplicationMetrics
{
    public const string MeterName = "MikroProje.API";
    private readonly Meter _meter;

    private readonly Counter<int> _salesCreatedCounter;
    private readonly Counter<int> _purchasesCreatedCounter;
    private readonly Counter<int> _failedLoginsCounter;
    private readonly Counter<int> _rateLimitRejectionsCounter;
    private readonly Counter<int> _dashboardCacheHitsCounter;
    private readonly Counter<int> _dashboardCacheMissesCounter;

    public MikroProjeMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);

        _salesCreatedCounter = _meter.CreateCounter<int>(
            "mikroproje.sales.created",
            description: "Toplam oluşturulan satış sayısı");

        _purchasesCreatedCounter = _meter.CreateCounter<int>(
            "mikroproje.purchases.created",
            description: "Toplam oluşturulan satın alma sayısı");

        _failedLoginsCounter = _meter.CreateCounter<int>(
            "mikroproje.logins.failed",
            description: "Başarısız login girişimleri");

        _rateLimitRejectionsCounter = _meter.CreateCounter<int>(
            "mikroproje.ratelimit.rejections",
            description: "Rate limit nedeniyle reddedilen istek sayısı");

        _dashboardCacheHitsCounter = _meter.CreateCounter<int>(
            "mikroproje.dashboard.cache.hits",
            description: "Dashboard cache hit sayısı");

        _dashboardCacheMissesCounter = _meter.CreateCounter<int>(
            "mikroproje.dashboard.cache.misses",
            description: "Dashboard cache miss sayısı");
    }

    public void IncrementSalesCreated(int count = 1) => _salesCreatedCounter.Add(count);
    public void IncrementPurchasesCreated(int count = 1) => _purchasesCreatedCounter.Add(count);
    public void IncrementFailedLogins(int count = 1) => _failedLoginsCounter.Add(count);
    public void IncrementRateLimitRejections(int count = 1) => _rateLimitRejectionsCounter.Add(count);
    public void IncrementDashboardCacheHits(int count = 1) => _dashboardCacheHitsCounter.Add(count);
    public void IncrementDashboardCacheMisses(int count = 1) => _dashboardCacheMissesCounter.Add(count);
}
