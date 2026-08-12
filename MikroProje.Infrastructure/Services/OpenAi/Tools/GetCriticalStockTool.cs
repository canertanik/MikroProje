using System.Text.Json;
using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Services.OpenAi.Tools;

public class GetCriticalStockTool : IErpToolHandler
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetCriticalStockTool(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public string ToolName => "get_critical_stock_products";

    public string Description => "Stoğu kritik seviyeye (minimum miktar altına) düşmüş ürünleri listeler.";

    public object ParametersSchema => System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>("""
    {
        "type": "object",
        "properties": {
            "maxCount": {
                "type": "integer",
                "description": "Maksimum getirilecek kayıt sayısı (en fazla 50)"
            }
        },
        "required": ["maxCount"],
        "additionalProperties": false
    }
    """);

    private class ToolArgs
    {
        public int? MaxCount { get; set; }
    }

    public async Task<string> ExecuteAsync(string argumentsJson, string userId, CancellationToken ct)
    {
        try
        {
            var args = string.IsNullOrWhiteSpace(argumentsJson)
                ? new ToolArgs()
                : JsonSerializer.Deserialize<ToolArgs>(argumentsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var max = args?.MaxCount ?? 10;
            if (max <= 0) max = 10;
            if (max > 50) max = 50;

            var criticalStocks = await _dashboardRepository.GetCriticalStockAsync(ct);
            var resultList = criticalStocks.Take(max).Select(c => new
            {
                c.ProductId,
                c.ProductCode,
                c.ProductName,
                c.CurrentStock,
                c.CriticalStock,
                c.Status
            });

            return JsonSerializer.Serialize(resultList, new JsonSerializerOptions
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
