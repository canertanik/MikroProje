using MediatR;
using Microsoft.AspNetCore.Mvc;
using MikroProje.Application.Features.SupplierStatements.Queries.GetSupplierStatement;
using Microsoft.AspNetCore.Authorization;

namespace MikroProje.API.Controllers;

[Authorize]
[Route("api/supplier-statements")]
[ApiController]
public class SupplierStatementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupplierStatementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{currentAccountId}")]
    public async Task<IActionResult> GetStatement(
        [FromRoute] int currentAccountId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSupplierStatementQuery
        {
            CurrentAccountId = currentAccountId,
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
