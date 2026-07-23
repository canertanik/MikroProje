using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.SupplierPayments.DTOs;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.SupplierPayments.Commands.CreateSupplierPayment;

public class CreateSupplierPaymentCommand : IRequest<Result<SupplierPaymentDto>>
{
    public int CurrentAccountId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public DateTime? PaymentDate { get; set; }
}
