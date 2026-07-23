using FluentValidation;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.Commands.CancelSale;
using MikroProje.Application.Features.Sales.Commands.CreateSale;
using MikroProje.Application.Features.Sales.Commands.UpdateSale;
using MikroProje.Application.Features.Sales.DTOs;
using MikroProje.Application.Features.Sales.Queries.GetAllSales;
using MikroProje.Application.Features.Sales.Queries.GetSaleById;
using MikroProje.Application.Features.Sales.Queries.GetSalesByCurrentAccount;
using Microsoft.AspNetCore.Mvc;

namespace MikroProje.API.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tüm satışları sayfalı listeler (iptal edilmiş satışlar hariç).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<SaleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(new GetAllSalesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            }, cancellationToken);

            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));
        }
    }

    /// <summary>
    /// Id'ye göre satış getirir.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<SaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetSaleByIdQuery { Id = id }, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));
        }
    }

    /// <summary>
    /// Cariye ait satışları sayfalı listeler.
    /// </summary>
    [HttpGet("current-account/{currentAccountId:int}")]
    [ProducesResponseType(typeof(Result<PagedResult<SaleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCurrentAccount(
        int currentAccountId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(new GetSalesByCurrentAccountQuery
            {
                CurrentAccountId = currentAccountId,
                PageNumber = pageNumber,
                PageSize = pageSize
            }, cancellationToken);

            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));
        }
    }

    /// <summary>
    /// Yeni satış oluşturur. Transaction: Sale + SaleDetail + StockOut + Stok güncelle + Bakiye artır.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<SaleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateSaleCommand command, CancellationToken cancellationToken)
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
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));
        }
    }

    /// <summary>
    /// Satış açıklamasını günceller (kalem değişikliği yapılamaz).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Result<SaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSaleCommand command, CancellationToken cancellationToken)
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
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));
        }
    }

    /// <summary>
    /// Satışı iptal eder. Transaction: StockIn (ters hareket) + Stok geri + Bakiye düşür + Soft delete.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new CancelSaleCommand { Id = id }, cancellationToken);
            if (result.StatusCode == StatusCodes.Status204NoContent)
            {
                return NoContent();
            }

            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));
        }
    }
}
