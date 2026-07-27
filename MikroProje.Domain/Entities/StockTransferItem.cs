using MikroProje.Domain.Common;

namespace MikroProje.Domain.Entities;

public class StockTransferItem : BaseEntity
{
    public int StockTransferId { get; set; }

    public StockTransfer StockTransfer { get; set; } = null!;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}
