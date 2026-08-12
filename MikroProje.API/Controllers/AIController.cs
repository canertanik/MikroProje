using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MikroProje.Application.Features.AI.DTOs;
using MikroProje.Application.Features.AI.Queries.GetProductForecast;
using MikroProje.Application.Interfaces;

namespace MikroProje.API.Controllers;

[Authorize]
[Route("api/ai")]
[ApiController]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOpenAiService _openAiService;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IDemandForecastService _demandForecastService;

    public AIController(
        IMediator mediator,
        IOpenAiService openAiService,
        IDashboardRepository dashboardRepository,
        IDemandForecastService demandForecastService)
    {
        _mediator = mediator;
        _openAiService = openAiService;
        _dashboardRepository = dashboardRepository;
        _demandForecastService = demandForecastService;
    }

    [HttpGet("forecast/products/{id}")]
    public async Task<IActionResult> GetProductForecast(int id)
    {
        var query = new GetProductForecastQuery { ProductId = id };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return BadRequest(new { Error = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpGet("insights/dashboard")]
    [EnableRateLimiting("AIInsights")]
    public async Task<IActionResult> GetDashboardInsights(CancellationToken ct)
    {
        var summary = await _dashboardRepository.GetSummaryAsync(null, null, ct);
        var criticalStocks = await _dashboardRepository.GetCriticalStockAsync(ct);
        
        var request = new DashboardInsightRequest
        {
            Summary = summary,
            CriticalStocks = criticalStocks.Take(10).ToList()
        };

        var result = await _openAiService.GetDashboardInsightsAsync(request, ct);

        if (!result.Success)
        {
            return BadRequest(new { Error = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost("chat")]
    [EnableRateLimiting("AIChat")]
    public async Task Chat([FromBody] ChatMessageDto request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            Response.StatusCode = 401;
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        await foreach (var chunk in _openAiService.ChatStreamAsync(request.Message, userId, ct))
        {
            var json = JsonSerializer.Serialize(chunk, jsonOpts);
            await Response.WriteAsync($"data: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
}
