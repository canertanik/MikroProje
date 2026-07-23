using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.Payments.DTOs;

public class PaymentDto
{
    public int Id { get; set; }

    public int CurrentAccountId { get; set; }

    public string CurrentAccountName { get; set; } = string.Empty;

    public string CurrentAccountCode { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentType Type { get; set; }

    public string TypeName { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; }

    public string PaymentMethodName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime PaymentDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
