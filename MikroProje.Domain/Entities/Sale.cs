using MikroProje.Domain.Common;

namespace MikroProje.Domain.Entities;

public class Sale : BaseEntity
{
    public int CurrentAccountId { get; set; }

    public CurrentAccount CurrentAccount { get; set; } = null!;

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public decimal VatAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public string? Description { get; set; }

    public ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();
}
