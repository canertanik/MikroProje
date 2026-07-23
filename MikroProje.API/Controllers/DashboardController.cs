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
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
