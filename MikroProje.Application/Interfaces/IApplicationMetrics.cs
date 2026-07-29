namespace MikroProje.Application.Interfaces;

public interface IApplicationMetrics
{
    void IncrementSalesCreated(int count = 1);
    void IncrementPurchasesCreated(int count = 1);
    void IncrementFailedLogins(int count = 1);
    void IncrementRateLimitRejections(int count = 1);
    void IncrementDashboardCacheHits(int count = 1);
    void IncrementDashboardCacheMisses(int count = 1);
}
