using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AI.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Services.OpenAi;

public class DisabledOpenAiService : IOpenAiService
{
    public Task<Result<DashboardInsightDto>> GetDashboardInsightsAsync(DashboardInsightRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result<DashboardInsightDto>.Ok(new DashboardInsightDto
        {
            Summary = "AI entegrasyonu şu an kapalı.",
            RiskExplanation = "Yapay zeka analizleri yapılandırma üzerinden devre dışı bırakılmış.",
            RecommendedAction = "Manuel inceleme yapınız.",
            Warnings = new List<string> { "AI disabled" }
        }));
    }

    public async IAsyncEnumerable<ChatStreamChunk> ChatStreamAsync(string userMessage, List<ChatHistoryItemDto>? history, string userId, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        yield return new ChatStreamChunk
        {
            Type = "error",
            Content = "AI asistanı yönetici tarafından devre dışı bırakılmış."
        };
        yield return new ChatStreamChunk { Type = "done" };
        
        await Task.CompletedTask;
    }
}
