using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Interfaces;

public class ForecastResultDto
{
    public float Forecast7Days { get; set; }
    public float Forecast30Days { get; set; }
    public float AverageDailyDemand { get; set; }
    public int EstimatedStockDays { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public int RecommendedPurchaseQuantity { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ForecastMetricsDto Metrics { get; set; } = new();
}

public class ForecastMetricsDto
{
    public float? Mae { get; set; }
    public float? Rmse { get; set; }
    public float? Mape { get; set; }
}

public interface IDemandForecastService
{
    Task<Result<ForecastResultDto>> GetProductForecastAsync(int productId, CancellationToken cancellationToken = default);
}
