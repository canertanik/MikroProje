using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.SupplierPayments.DTOs;

public class SupplierPaymentListDto
{
    public int Id { get; set; }
    public int CurrentAccountId { get; set; }
    public string CurrentAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
}
