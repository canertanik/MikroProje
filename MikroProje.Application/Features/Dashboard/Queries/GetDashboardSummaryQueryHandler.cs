using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Dashboard.Queries;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardSummaryQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow; // Or project specific standard
        
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);
        
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);

        var result = await _dashboardRepository.GetSummaryAsync(todayStart, tomorrowStart, monthStart, nextMonthStart, cancellationToken);
        
        return Result<DashboardSummaryDto>.Ok(result);
    }
}
