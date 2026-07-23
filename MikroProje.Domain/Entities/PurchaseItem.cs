using MikroProje.Domain.Common;

namespace MikroProje.Domain.Entities;

public class PurchaseItem : BaseEntity
{
    public int PurchaseId { get; set; }

    public Purchase Purchase { get; set; } = null!;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal VatRate { get; set; }

    public decimal LineSubtotal { get; set; }

    public decimal VatAmount { get; set; }

    public decimal LineTotal { get; set; }
}
