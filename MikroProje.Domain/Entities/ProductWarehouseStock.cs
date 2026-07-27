using MikroProje.Domain.Common;

namespace MikroProje.Domain.Entities;

public class ProductWarehouseStock : BaseEntity, IAuditIgnore
{
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public int Quantity { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
