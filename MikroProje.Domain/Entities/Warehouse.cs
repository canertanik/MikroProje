using MikroProje.Domain.Common;

namespace MikroProje.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<ProductWarehouseStock> ProductWarehouseStocks { get; set; } = new List<ProductWarehouseStock>();
}
