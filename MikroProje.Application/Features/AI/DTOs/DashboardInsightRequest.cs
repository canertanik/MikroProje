using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.AI.DTOs;

/// <summary>
/// OpenAI'a gönderilecek gerçek ERP verileri — AI bu verilerden insight üretir.
/// </summary>
public class DashboardInsightRequest
{
    public DashboardSummaryDto Summary { get; set; } = new();
    public List<DashboardCriticalStockDto> CriticalStocks { get; set; } = new();
    public List<ForecastResultDto>? Forecasts { get; set; }
}
