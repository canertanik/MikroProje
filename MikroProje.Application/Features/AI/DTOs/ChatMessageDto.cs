namespace MikroProje.Application.Features.AI.DTOs;

public class ChatMessageDto
{
    public string Message { get; set; } = string.Empty;
    public List<ChatHistoryItemDto> History { get; set; } = new();
}

public class ChatHistoryItemDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
