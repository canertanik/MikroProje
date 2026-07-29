namespace MikroProje.API.Options;

public class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; set; } = "MikroProje.API";
    public SerilogOptions Serilog { get; set; } = new();
    public OpenTelemetryOptions OpenTelemetry { get; set; } = new();
}

public class SerilogOptions
{
    public bool EnableFileLogging { get; set; } = true;
    public string LogDirectory { get; set; } = "logs";
    public int RetainedFileCountLimit { get; set; } = 14;
}

public class OpenTelemetryOptions
{
    public bool EnableTracing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableConsoleExporter { get; set; } = false;
    public bool EnableOtlpExporter { get; set; } = false;
    public string OtlpEndpoint { get; set; } = "";
    public double SamplingRatio { get; set; } = 1.0;
}
