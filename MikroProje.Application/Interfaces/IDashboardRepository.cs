using MikroProje.Application.Features.Dashboard.DTOs;

namespace MikroProje.Application.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken);

    Task<List<DashboardTrendsDto>> GetTrendsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken);

    Task<List<DashboardRecentActivityDto>> GetRecentActivitiesAsync(
        CancellationToken cancellationToken);

    Task<List<DashboardCriticalStockDto>> GetCriticalStockAsync(
        CancellationToken cancellationToken);

    Task<DashboardTopRecordsDto> GetTopRecordsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken);
}
