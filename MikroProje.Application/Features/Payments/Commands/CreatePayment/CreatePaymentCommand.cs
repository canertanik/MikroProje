using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Payments.DTOs;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommand : IRequest<Result<PaymentDto>>
{
    public int CurrentAccountId { get; set; }

    public decimal Amount { get; set; }

    /// <summary>
    /// 1 = Collection (Tahsilat), 2 = Payment (Ödeme)
    /// </summary>
    public PaymentType Type { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? Description { get; set; }

    public DateTime PaymentDate { get; set; }
}
