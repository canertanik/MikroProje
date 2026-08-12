using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Payments.DTOs;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.Payments.Commands.UpdatePayment;

public class UpdatePaymentCommand : IRequest<Result<PaymentDto>>
{
    public int Id { get; set; }

    public int CurrentAccountId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? Description { get; set; }

    public DateTime PaymentDate { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
