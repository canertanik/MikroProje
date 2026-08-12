using System.Text.Json;
using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Services.OpenAi.Tools;

public class GetTopSellingProductsTool : IErpToolHandler
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetTopSellingProductsTool(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public string ToolName => "get_top_selling_products";

    public string Description => "Son dönemdeki en çok satan ürünleri listeler.";

    public object ParametersSchema => System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>("""
    {
        "type": "object",
        "properties": {
            "maxCount": {
                "type": "integer",
                "description": "Maksimum getirilecek kayıt sayısı (en fazla 20)"
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

            var max = args?.MaxCount ?? 5;
            if (max <= 0) max = 5;
            if (max > 20) max = 20;

            // Get records for the last 30 days
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30);
            
            var topRecords = await _dashboardRepository.GetTopRecordsAsync(startDate, endDate, ct);
            var resultList = topRecords.TopProductsBySales.Take(max).Select(p => new
            {
                p.ProductName,
                p.TotalQuantitySold
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
