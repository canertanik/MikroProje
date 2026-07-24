using FluentValidation;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.Commands.CreateProduct;
using MikroProje.Application.Features.Products.Commands.DeleteProduct;
using MikroProje.Application.Features.Products.Commands.UpdateProduct;
using MikroProje.Application.Features.Products.DTOs;
using MikroProje.Application.Features.Products.Queries.GetAllProducts;
using MikroProje.Application.Features.Products.Queries.GetCriticalStockProducts;
using MikroProje.Application.Features.Products.Queries.GetProductById;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MikroProje.API.Controllers;

[Authorize]
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? criticalOnly, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(new GetAllProductsQuery
            {
                Search = search,
                CriticalOnly = criticalOnly,
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

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id }, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())));
        }
    }

    [HttpGet("critical")]
    [ProducesResponseType(typeof(Result<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCritical([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(new GetCriticalStockProductsQuery
            {
                Search = search,
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

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (result.StatusCode == StatusCodes.Status201Created && result.Data is not null)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
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

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new DeleteProductCommand { Id = id }, cancellationToken);
            if (result.StatusCode == StatusCodes.Status204NoContent)
            {
                return NoContent();
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
}
