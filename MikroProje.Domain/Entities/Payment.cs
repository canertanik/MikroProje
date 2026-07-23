using MikroProje.Domain.Common;
using MikroProje.Domain.Enums;

namespace MikroProje.Domain.Entities;

public class Payment : BaseEntity
{
    public int CurrentAccountId { get; set; }

    public CurrentAccount CurrentAccount { get; set; } = null!;

    public decimal Amount { get; set; }

    public PaymentType Type { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? Description { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
