using System.Text.Json;
using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Services.OpenAi.Tools;

public class GetProductForecastTool : IErpToolHandler
{
    private readonly IDemandForecastService _demandForecastService;

    public GetProductForecastTool(IDemandForecastService demandForecastService)
    {
        _demandForecastService = demandForecastService;
    }

    public string ToolName => "get_product_forecast";

    public string Description => "Belirli bir ürün için ML (Makine Öğrenimi) destekli 30 günlük satış talebi tahminlerini getirir.";

    public object ParametersSchema => System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>("""
    {
        "type": "object",
        "properties": {
            "productId": {
                "type": "integer",
                "description": "Tahmin alınacak ürünün ID'si"
            }
        },
        "required": ["productId"],
        "additionalProperties": false
    }
    """);

    private class ToolArgs
    {
        public int ProductId { get; set; }
    }

    public async Task<string> ExecuteAsync(string argumentsJson, string userId, CancellationToken ct)
    {
        try
        {
            System.Console.WriteLine($"[DEBUG] get_product_forecast called with: {argumentsJson}");
            var args = JsonSerializer.Deserialize<ToolArgs>(argumentsJson, new JsonSerializerOptions { 
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            });
            
            if (args == null || args.ProductId <= 0)
            {
                return JsonSerializer.Serialize(new { error = "Geçerli bir productId (sıfırdan büyük) sağlanmalıdır. Gelen Argüman: " + argumentsJson });
            }

            var forecastResult = await _demandForecastService.GetProductForecastAsync(args.ProductId, ct);
            
            if (!forecastResult.Success || forecastResult.Data == null)
            {
                return JsonSerializer.Serialize(new { error = forecastResult.Message ?? "Tahmin alınamadı." });
            }

            // Minimal DTO dönüyoruz
            return JsonSerializer.Serialize(new
            {
                forecastResult.Data.Forecast7Days,
                forecastResult.Data.Forecast30Days,
                forecastResult.Data.AverageDailyDemand,
                forecastResult.Data.EstimatedStockDays,
                forecastResult.Data.RiskLevel,
                forecastResult.Data.RecommendedPurchaseQuantity,
                forecastResult.Data.ModelUsed,
                forecastResult.Data.Confidence,
                forecastResult.Data.Message,
                Metrics = new 
                {
                    forecastResult.Data.Metrics?.Mae,
                    forecastResult.Data.Metrics?.Rmse,
                    forecastResult.Data.Metrics?.Mape
                }
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
