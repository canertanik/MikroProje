using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.SupplierPayments.Commands.CreateSupplierPayment;
using MikroProje.Application.Features.SupplierPayments.DTOs;
using MikroProje.Application.Features.SupplierPayments.Queries.GetAllSupplierPayments;
using MikroProje.Application.Features.SupplierPayments.Queries.GetSupplierPaymentById;
using Microsoft.AspNetCore.Mvc;

namespace MikroProje.API.Controllers;

[ApiController]
[Route("api/supplier-payments")]
public class SupplierPaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupplierPaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<SupplierPaymentDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierPaymentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.StatusCode == StatusCodes.Status201Created)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<SupplierPaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSupplierPaymentByIdQuery { Id = id }, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<SupplierPaymentListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllSupplierPaymentsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
