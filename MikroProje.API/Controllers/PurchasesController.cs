using MediatR;
using Microsoft.AspNetCore.Mvc;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.Commands.CreatePurchase;
using MikroProje.Application.Features.Purchases.DTOs;
using MikroProje.Application.Features.Purchases.Queries.GetAllPurchases;
using MikroProje.Application.Features.Purchases.Queries.GetPurchaseById;

namespace MikroProje.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PurchasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<PurchaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PurchaseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPurchaseByIdQuery { Id = id }, cancellationToken);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<PurchaseListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllPurchasesQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<PurchaseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<PurchaseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<PurchaseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<PurchaseDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Result<PurchaseDto>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
}
