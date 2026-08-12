namespace MikroProje.Application.Features.AI.DTOs;

public class ChatStreamChunk
{
    /// <summary>
    /// "text_delta" | "error" | "done" | "usage"
    /// </summary>
    public string Type { get; set; } = "text_delta";
    public string? Content { get; set; }
    public ChatUsageInfo? Usage { get; set; }
}

public class ChatUsageInfo
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}
