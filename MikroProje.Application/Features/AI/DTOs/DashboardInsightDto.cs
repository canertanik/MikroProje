namespace MikroProje.Application.Features.AI.DTOs;

/// <summary>
/// Dashboard AI Insight sonucu — OpenAI Structured Output olarak döner.
/// </summary>
public class DashboardInsightDto
{
    public string Summary { get; set; } = string.Empty;
    public string RiskExplanation { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}
