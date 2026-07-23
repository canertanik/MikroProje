using MikroProje.Domain.Common;
using MikroProje.Domain.Enums;

namespace MikroProje.Domain.Entities;

public class CurrentAccount : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? TaxNumber { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public CurrentAccountType Type { get; set; }

    public decimal Balance { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
