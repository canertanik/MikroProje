using MikroProje.Domain.Common;

namespace MikroProje.Domain.Entities;

public class SaleDetail : BaseEntity
{
    public int SaleId { get; set; }

    public Sale Sale { get; set; } = null!;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal VatRate { get; set; }

    public decimal Discount { get; set; }

    public decimal LineTotal { get; set; }
}
