using MikroProje.Domain.Common;
using MikroProje.Domain.Enums;

namespace MikroProje.Domain.Entities;

public class SupplierPayment : BaseEntity
{
    public int CurrentAccountId { get; set; }

    public CurrentAccount CurrentAccount { get; set; } = null!;

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }
}
