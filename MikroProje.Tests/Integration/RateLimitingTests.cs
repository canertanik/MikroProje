using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MikroProje.Tests.Integration;

public class RateLimitingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RateLimitingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var inMemorySettings = new Dictionary<string, string?>
                {
                    {"RateLimiting:Global:PermitLimit", "2"},
                    {"RateLimiting:Global:WindowSeconds", "2"},
                    {"RateLimiting:Global:QueueLimit", "0"},
                    {"RateLimiting:Login:PermitLimit", "1"},
                    {"RateLimiting:Login:WindowSeconds", "2"},
                    {"RateLimiting:Login:QueueLimit", "0"}
                };

                configBuilder.AddInMemoryCollection(inMemorySettings);
            });
        });
    }

    [Fact]
    public async Task GlobalLimit_ShouldAcceptRequests_UnderLimit()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.100");

        var response1 = await client.GetAsync("/health");
        var response2 = await client.GetAsync("/health");

        response1.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        response2.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task GlobalLimit_ShouldReturn429_WhenLimitExceeded()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.101");

        await client.GetAsync("/health");
        await client.GetAsync("/health");
        var response3 = await client.GetAsync("/health");

        response3.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response3.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        
        var content = await response3.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Contain("Sistem limitlerini aştınız");

        response3.Headers.Contains("Retry-After").Should().BeTrue();
    }

    [Fact]
    public async Task LoginLimit_ShouldBeStricterThanGlobalLimit()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.102");

        var loginPayload = new { Email = "test@test.com", Password = "123" };
        var response1 = await client.PostAsJsonAsync("/api/auth/login", loginPayload);
        var response2 = await client.PostAsJsonAsync("/api/auth/login", loginPayload);

        // Limit is 1 for login, so second request should be 429
        response1.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        response2.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
    
    [Fact]
    public async Task DifferentIPs_ShouldHaveSeparatePartitions()
    {
        var client1 = _factory.CreateClient();
        client1.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.103");
        
        var client2 = _factory.CreateClient();
        client2.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.104");

        // client1 uses all its limit
        await client1.GetAsync("/health");
        await client1.GetAsync("/health");
        var response1 = await client1.GetAsync("/health");
        response1.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        // client2 should still be able to make requests (limit is 2)
        var response2 = await client2.GetAsync("/health");
        response2.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task WindowExpiration_ShouldAcceptRequestsAgain()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.105");

        // Use up the limit (2)
        await client.GetAsync("/health");
        await client.GetAsync("/health");
        var response1 = await client.GetAsync("/health");
        response1.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        // Wait for the window to expire (2 seconds + a small buffer)
        await Task.Delay(TimeSpan.FromSeconds(2.5));

        // Should be accepted again
        var response2 = await client.GetAsync("/health");
        response2.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }
}
