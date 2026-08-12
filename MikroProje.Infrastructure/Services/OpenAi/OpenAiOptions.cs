namespace MikroProje.Infrastructure.Services.OpenAi;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";
    public bool Enabled { get; set; } = true;
    public string Model { get; set; } = "gpt-4o-mini";
    public int MaxOutputTokens { get; set; } = 2048;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxToolCallIterations { get; set; } = 5;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
}
