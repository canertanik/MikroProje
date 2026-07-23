using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.StockMovements.DTOs;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.StockMovements.Commands.CreateStockMovement;

public class CreateStockMovementCommand : IRequest<Result<StockMovementDto>>
{
    public int ProductId { get; set; }

    public StockMovementType MovementType { get; set; }

    public StockMovementSourceType SourceType { get; set; }

    public int Quantity { get; set; }

    public string? DocumentNumber { get; set; }

    public string? Description { get; set; }

    public DateTime? MovementDate { get; set; }
}
