using FluentValidation;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.StockMovements.Commands.CreateStockMovement;
using MikroProje.Application.Features.StockMovements.DTOs;
using MikroProje.Application.Features.StockMovements.Queries.GetStockMovementsByProduct;
using MikroProje.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MikroProje.API.Controllers;

[ApiController]
[Route("api/stock-movements")]
public class StockMovementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockMovementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<StockMovementDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (result.StatusCode == StatusCodes.Status201Created)
            {
                return StatusCode(StatusCodes.Status201Created, result);
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())));
        }
    }

    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(Result<PagedResult<StockMovementDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProduct(int productId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] StockMovementType? movementType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(new GetStockMovementsByProductQuery
            {
                ProductId = productId,
                StartDate = startDate,
                EndDate = endDate,
                MovementType = movementType,
                PageNumber = pageNumber,
                PageSize = pageSize
            }, cancellationToken);

            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())));
        }
    }
}
