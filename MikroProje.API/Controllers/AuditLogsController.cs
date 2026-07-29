using MikroProje.Application.Features.AuditLogs.Queries.ExportAuditLogsPdf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AuditLogs.DTOs;
using MikroProje.Application.Features.AuditLogs.Queries.GetAllAuditLogs;
using MikroProje.Application.Features.AuditLogs.Queries.GetAuditLogById;
using MikroProje.Domain.Enums;

namespace MikroProje.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<AuditLogListDto>>>> GetAll(
        [FromQuery] string? userId,
        [FromQuery] string? username,
        [FromQuery] string? entityName,
        [FromQuery] string? entityId,
        [FromQuery] AuditAction? action,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllAuditLogsQuery
        {
            UserId = userId,
            Username = username,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            StartDate = startDate,
            EndDate = endDate,
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<AuditLogDto>>> GetById(int id)
    {
        var query = new GetAuditLogByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return StatusCode(result.StatusCode, result);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] ExportAuditLogsPdfQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.Success && result.Data != null)
        {
            return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
        }
        return BadRequest(result.Message);
    }
}




