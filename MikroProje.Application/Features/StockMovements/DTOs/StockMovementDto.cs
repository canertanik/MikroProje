using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.StockMovements.DTOs;

public class StockMovementDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public StockMovementType MovementType { get; set; }

    public StockMovementSourceType SourceType { get; set; }

    public int Quantity { get; set; }

    public int PreviousQuantity { get; set; }

    public int NewQuantity { get; set; }

    public string? DocumentNumber { get; set; }

    public string? Description { get; set; }

    public DateTime MovementDate { get; set; }

    public DateTime CreatedDate { get; set; }
}
