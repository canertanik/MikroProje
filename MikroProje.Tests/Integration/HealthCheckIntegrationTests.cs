using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MikroProje.Tests.Integration;

public class HealthCheckIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthCheckIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LiveEndpoint_ShouldReturnOk_AndNotRequireAuthentication()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        Assert.True(root.TryGetProperty("status", out var statusProp));
        Assert.Equal("Healthy", statusProp.GetString());
        
        // Sadece live kontrolünü içermeli
        Assert.True(root.TryGetProperty("checks", out var checksProp));
        Assert.Contains(checksProp.EnumerateArray(), c => c.GetProperty("name").GetString() == "live");
        Assert.DoesNotContain(checksProp.EnumerateArray(), c => c.GetProperty("name").GetString() == "sqlserver");
        Assert.DoesNotContain(checksProp.EnumerateArray(), c => c.GetProperty("name").GetString() == "redis");
    }

    [Fact]
    public async Task ReadyEndpoint_ShouldReturnValidJson()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        // We don't assert 200 OK here because in CI/test environment without Redis/SQL, it might return 503 Unhealthy.
        // We just verify it returns a valid JSON structure.
        
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("status", out _));
        Assert.True(root.TryGetProperty("totalDuration", out _));
        Assert.True(root.TryGetProperty("checks", out var checksProp));
        
        // Ready endpoint'i sqlserver ve redis içermeli
        Assert.Contains(checksProp.EnumerateArray(), c => c.GetProperty("name").GetString() == "sqlserver");
        Assert.Contains(checksProp.EnumerateArray(), c => c.GetProperty("name").GetString() == "redis");
    }

    [Fact]
    public async Task RootHealthEndpoint_ShouldReturnValidJson()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("status", out _));
        Assert.True(root.TryGetProperty("totalDuration", out _));
        Assert.True(root.TryGetProperty("checks", out var checksProp));
        
        // Root endpoint'i tümünü içermeli
        Assert.Contains(checksProp.EnumerateArray(), c => c.GetProperty("name").GetString() == "live");
        Assert.Contains(checksProp.EnumerateArray(), c => c.GetProperty("name").GetString() == "sqlserver");
        Assert.Contains(checksProp.EnumerateArray(), c => c.GetProperty("name").GetString() == "redis");
    }
}
