using FluentValidation;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.Warehouses.Commands.CreateWarehouse;
using MikroProje.Application.Features.Warehouses.Commands.DeleteWarehouse;
using MikroProje.Application.Features.Warehouses.Commands.SetDefaultWarehouse;
using MikroProje.Application.Features.Warehouses.Commands.UpdateWarehouse;
using MikroProje.Application.Features.Warehouses.DTOs;
using MikroProje.Application.Features.Warehouses.Queries.GetAllWarehouses;
using MikroProje.Application.Features.Warehouses.Queries.GetWarehouseById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MikroProje.API.Controllers;

[Authorize]
[ApiController]
[Route("api/warehouses")]
public class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<WarehouseListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllWarehousesQuery query, CancellationToken cancellationToken)
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
    [ProducesResponseType(typeof(Result<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetWarehouseByIdQuery { Id = id }, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(Result<WarehouseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateWarehouseCommand { Dto = request };
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
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Result<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateWarehouseCommand { Id = id, Dto = request };
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new DeleteWarehouseCommand { Id = id }, cancellationToken);

            if (result.StatusCode == StatusCodes.Status204NoContent)
            {
                return NoContent();
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/set-default")]
    [ProducesResponseType(typeof(Result<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new SetDefaultWarehouseCommand { Id = id }, cancellationToken);
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
