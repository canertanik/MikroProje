using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.StockTransfers.Commands.CancelStockTransfer;
using MikroProje.Application.Features.StockTransfers.Commands.CompleteStockTransfer;
using MikroProje.Application.Features.StockTransfers.Commands.CreateStockTransfer;
using MikroProje.Application.Features.StockTransfers.DTOs;
using MikroProje.Application.Features.StockTransfers.Queries.GetAllStockTransfers;
using MikroProje.Application.Features.StockTransfers.Queries.GetStockTransferById;

namespace MikroProje.API.Controllers;

[Authorize]
[ApiController]
[Route("api/stock-transfers")]
public class StockTransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockTransfersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<StockTransferListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllStockTransfersQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(query, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<StockTransferDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetStockTransferByIdQuery { Id = id }, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(Result<StockTransferDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStockTransferRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateStockTransferCommand { Dto = request };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.StatusCode == StatusCodes.Status201Created && result.Data is not null)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteStockTransferCommand command, CancellationToken cancellationToken)
    {
        try
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelStockTransferCommand command, CancellationToken cancellationToken)
    {
        try
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    private static IDictionary<string, string[]> CreateValidationErrors(ValidationException exception)
    {
        return exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
    }

    private ValidationProblemDetails CreateValidationProblemDetails(ValidationException exception)
    {
        return new ValidationProblemDetails(CreateValidationErrors(exception));
    }
}
