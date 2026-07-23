using MikroProje.Application.Features.Dashboard.DTOs;

namespace MikroProje.Application.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(
        DateTime todayStart,
        DateTime tomorrowStart,
        DateTime monthStart,
        DateTime nextMonthStart,
        CancellationToken cancellationToken);
}
