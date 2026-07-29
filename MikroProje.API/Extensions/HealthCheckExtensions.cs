using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MikroProje.API.Extensions;

public static class HealthCheckExtensions
{
    public static Task WriteResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonWriterOptions { Indented = true };

        using var memoryStream = new MemoryStream();
        using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("status", healthReport.Status.ToString());
            jsonWriter.WriteString("totalDuration", healthReport.TotalDuration.ToString());
            
            jsonWriter.WriteStartArray("checks");
            
            foreach (var healthReportEntry in healthReport.Entries)
            {
                jsonWriter.WriteStartObject();
                
                // Key (name) will be mapped appropriately, e.g. "sqlserver" or "redis"
                jsonWriter.WriteString("name", healthReportEntry.Key.ToLower());
                jsonWriter.WriteString("status", healthReportEntry.Value.Status.ToString());
                jsonWriter.WriteString("duration", healthReportEntry.Value.Duration.ToString());
                
                if (healthReportEntry.Value.Description != null)
                {
                    jsonWriter.WriteString("description", healthReportEntry.Value.Description);
                }
                else
                {
                    jsonWriter.WriteNull("description");
                }
                
                jsonWriter.WriteEndObject();
            }
            
            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();
        }

        return context.Response.WriteAsync(System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()));
    }
}
