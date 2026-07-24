using FluentValidation;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.Commands.CreateCurrentAccount;
using MikroProje.Application.Features.CurrentAccounts.Commands.DeleteCurrentAccount;
using MikroProje.Application.Features.CurrentAccounts.Commands.UpdateCurrentAccount;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Application.Features.CurrentAccounts.Queries.GetAllCurrentAccounts;
using MikroProje.Application.Features.CurrentAccounts.Queries.GetCurrentAccountById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MikroProje.API.Controllers;

[Authorize]
[ApiController]
[Route("api/current-accounts")]
public class CurrentAccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CurrentAccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyCollection<CurrentAccountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetAllCurrentAccountsQuery(), cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<CurrentAccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetCurrentAccountByIdQuery { Id = id }, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(Result<CurrentAccountDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCurrentAccountCommand command, CancellationToken cancellationToken)
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
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Result<CurrentAccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCurrentAccountCommand command, CancellationToken cancellationToken)
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
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new DeleteCurrentAccountCommand { Id = id }, cancellationToken);

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

    [HttpGet("{id:int}/statement")]
    [ProducesResponseType(typeof(Result<MikroProje.Application.Common.Pagination.PagedResult<StatementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatement(
        int id, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new MikroProje.Application.Features.CurrentAccounts.Queries.GetStatement.GetCurrentAccountStatementQuery
            {
                CurrentAccountId = id,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        catch (ValidationException exception)
        {
            return ValidationProblem(CreateValidationProblemDetails(exception));
        }
    }
}
