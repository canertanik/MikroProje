namespace MikroProje.API.Options;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public RateLimitPolicyOptions Global { get; set; } = new();
    public RateLimitPolicyOptions Login { get; set; } = new();
}

public class RateLimitPolicyOptions
{
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 0;
}
