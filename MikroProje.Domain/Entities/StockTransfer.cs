using MikroProje.Domain.Common;
using MikroProje.Domain.Enums;

namespace MikroProje.Domain.Entities;

public class StockTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty;

    public int SourceWarehouseId { get; set; }

    public Warehouse SourceWarehouse { get; set; } = null!;

    public int DestinationWarehouseId { get; set; }

    public Warehouse DestinationWarehouse { get; set; } = null!;

    public DateTime TransferDate { get; set; } = DateTime.UtcNow;

    public string? Description { get; set; }

    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}
