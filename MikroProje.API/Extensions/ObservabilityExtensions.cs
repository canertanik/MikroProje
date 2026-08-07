using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using MikroProje.API.Options;

namespace MikroProje.API.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddCustomObservability(this IServiceCollection services, WebApplicationBuilder builder)
    {
        var options = builder.Configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>() ?? new ObservabilityOptions();
        
        services.Configure<ObservabilityOptions>(builder.Configuration.GetSection(ObservabilityOptions.SectionName));

        // 1. Serilog Setup
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", options.ServiceName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);

        if (builder.Environment.IsDevelopment())
        {
            loggerConfig.WriteTo.Console(); // Normal readable text for devs
        }
        else
        {
            loggerConfig.WriteTo.Console(new RenderedCompactJsonFormatter());
        }

        if (options.Serilog.EnableFileLogging)
        {
            loggerConfig.WriteTo.File(
                new RenderedCompactJsonFormatter(),
                Path.Combine(options.Serilog.LogDirectory, "mikroproje-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: options.Serilog.RetainedFileCountLimit);
        }

        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();

        // 2. OpenTelemetry Setup
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: options.ServiceName, serviceVersion: "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["environment.name"] = builder.Environment.EnvironmentName
                }))
            .WithTracing(tracing =>
            {
                if (options.OpenTelemetry.EnableTracing)
                {
                    tracing
                        .AddAspNetCoreInstrumentation(opts => 
                        {
                            opts.RecordException = true;
                        })
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddRedisInstrumentation();

                    if (options.OpenTelemetry.SamplingRatio < 1.0)
                    {
                        tracing.SetSampler(new TraceIdRatioBasedSampler(options.OpenTelemetry.SamplingRatio));
                    }
                    else
                    {
                        tracing.SetSampler(new AlwaysOnSampler());
                    }

                    if (options.OpenTelemetry.EnableConsoleExporter && builder.Environment.IsDevelopment())
                    {
                        tracing.AddConsoleExporter();
                    }

                    if (options.OpenTelemetry.EnableOtlpExporter && !string.IsNullOrWhiteSpace(options.OpenTelemetry.OtlpEndpoint))
                    {
                        tracing.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.OpenTelemetry.OtlpEndpoint);
                        });
                    }
                }
            })
            .WithMetrics(metrics =>
            {
                if (options.OpenTelemetry.EnableMetrics)
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddMeter(MikroProje.Infrastructure.Observability.MikroProjeMetrics.MeterName);

                    if (options.OpenTelemetry.EnableConsoleExporter && builder.Environment.IsDevelopment())
                    {
                        metrics.AddConsoleExporter();
                    }

                    if (options.OpenTelemetry.EnableOtlpExporter && !string.IsNullOrWhiteSpace(options.OpenTelemetry.OtlpEndpoint))
                    {
                        metrics.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.OpenTelemetry.OtlpEndpoint);
                        });
                    }
                }
            });

        return services;
    }
}
