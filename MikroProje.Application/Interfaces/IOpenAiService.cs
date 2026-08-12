using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AI.DTOs;

namespace MikroProje.Application.Interfaces;

public interface IOpenAiService
{
    /// <summary>
    /// Dashboard AI Insights — Structured JSON, streaming yok.
    /// </summary>
    Task<Result<DashboardInsightDto>> GetDashboardInsightsAsync(
        DashboardInsightRequest request, CancellationToken ct);

    /// <summary>
    /// Chat Assistant — SSE streaming ile parça parça text döner.
    /// Tool calling loop'u servis içinde yönetilir.
    /// </summary>
    IAsyncEnumerable<ChatStreamChunk> ChatStreamAsync(
        string userMessage, List<ChatHistoryItemDto>? history, string userId, CancellationToken ct);
}
