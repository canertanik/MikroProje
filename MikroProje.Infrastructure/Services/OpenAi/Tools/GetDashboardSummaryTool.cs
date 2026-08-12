using System.Text.Json;
using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Services.OpenAi.Tools;

public class GetDashboardSummaryTool : IErpToolHandler
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardSummaryTool(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public string ToolName => "get_dashboard_summary";

    public string Description => "Dashboard üzerinden genel satış, alış, kasa ve stok özetlerini getirir. Parametre almaz.";

    public object ParametersSchema => System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>("""
    {
        "type": "object",
        "properties": { },
        "additionalProperties": false
    }
    """);

    public async Task<string> ExecuteAsync(string argumentsJson, string userId, CancellationToken ct)
    {
        try
        {
            // Bu sorgu genel özet döner, tüm kullanıcılar tarafından görülebilir
            // İleride role-based filtre eklenecekse userId kullanılabilir.
            
            var summary = await _dashboardRepository.GetSummaryAsync(null, null, ct);
            
            // Minimal DTO olarak direkt DTO dönebilir, entity değil.
            return JsonSerializer.Serialize(summary, new JsonSerializerOptions
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
