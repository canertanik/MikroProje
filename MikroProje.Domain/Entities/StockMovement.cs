using MikroProje.Domain.Common;
using MikroProje.Domain.Enums;

namespace MikroProje.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public StockMovementType MovementType { get; set; }

    public StockMovementSourceType SourceType { get; set; }

    public int Quantity { get; set; }

    public int PreviousQuantity { get; set; }

    public int NewQuantity { get; set; }

    public string? DocumentNumber { get; set; }

    public string? Description { get; set; }

    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
}