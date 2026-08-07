using MediatR;
using Microsoft.AspNetCore.Mvc;
using MikroProje.Application.Features.Dashboard.Queries;

using Microsoft.AspNetCore.Authorization;

namespace MikroProje.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery { StartDate = startDate, EndDate = endDate }, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardTrendsQuery { StartDate = startDate, EndDate = endDate }, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("recent-activities")]
    public async Task<IActionResult> GetRecentActivities(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRecentActivitiesQuery(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("critical-stock")]
    public async Task<IActionResult> GetCriticalStock(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCriticalStockQuery(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("top-records")]
    public async Task<IActionResult> GetTopRecords([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTopRecordsQuery { StartDate = startDate, EndDate = endDate }, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
