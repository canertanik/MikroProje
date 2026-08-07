using MikroProje.Domain.Common;
using MikroProje.Domain.Enums;

namespace MikroProje.Domain.Entities;

public class Purchase : BaseEntity
{
    public int CurrentAccountId { get; set; }

    public CurrentAccount CurrentAccount { get; set; } = null!;

    public int WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public decimal Subtotal { get; set; }

    public decimal VatAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public string? Description { get; set; }

    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

    public DateTime? ReceivedDate { get; set; }

    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
}
